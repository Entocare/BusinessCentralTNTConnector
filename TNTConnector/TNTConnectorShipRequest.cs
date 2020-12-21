using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Threading.Tasks;
using BusinessCentralTNTConnector.TNTConnector.PrintHTML;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// Deze TNTConnector wrapt een enkel "ShipRequest" (>> accesscode) en doet ook alle vervolgacties
    /// - GET_RESULT om het resultaatdocument op te halen en het daar na te ontcijferen
    /// - GET_<doctype> om de documentatie op te halen en met Internet Explorer (of Edge) op het scherm te zetten
    /// 
    /// Hij weet of we een "full" of een "printonly" verzoek gaan doen.
    /// De AddConsignment methode checkt of de ontvangen zending rij (gescrolde recordset) klopt met het type verzoek:
    /// Bij "full" zijn er nog geen ConsignmentIDs, bij "printonly" moeten die er wel zijn!
    /// Per ShipRequest worden deze statussend doorlopen:
    /// - Initieel (addConsignment mogelijk): opbouwen van het initiele verzoek in xml
    /// - Aanmelden (>> accesscode)
    /// - Get_Result (>> ConsignmentID en per doctype: created of niet
    /// - Get_Docs
    /// - Ready
    /// 
    /// Fase "aanmelden" eindigt met event "Accesscode_received()" of met een Error_event
    /// Fase "GET_RESULT" eindigt met event "Result_received(ConsignmentID, LabelCreated, ManifestCreated, ConnoteCreated, InvoiceCreated)"
    /// of een Error_event
    /// Ieder van de doc-fetches eindigt met een doc-specifiek event: Label_toscreen, Manifest_toscreen, ... etc.
    /// of een doc-specifieke foutmelding
    /// Tenslotte is er een "Requests_ready" event als er geen follow-up requests meer lopen.
    /// </summary>
    public class TNTConnectorShipRequest : IDisposable
    {
        public enum Statusses { BuildRequest, SendRequest, GetResult, GetDocs, Ready }
        public enum DocStatusses { Initial, OnTNTServer, Fetching, Fetched }
        public enum DocTypes { Label = 0, Manifest = 1, Connote = 2, Invoice = 3 }

        //Events
        public event EventHandler AccessCodeReceived;
        public event EventHandler<ConsignmentSuccessArgs> ConsignmentSuccess;
        public event EventHandler<ResultReceivedArgs> ResultReceived;
        public event EventHandler<DocEventArgs> DocumentRequest;
        public event EventHandler<DocEventArgs> DocumentReceived;
        public event EventHandler ShipRequestReady;   //all follow up requests finished
        //error events
        public event EventHandler<ErrorArgs> PostError;  //no response or invalid response
        public event EventHandler<ErrorArgs> ShipRequestError;
        public event EventHandler<ErrorArgs> ShipResultError;
        public event EventHandler<ConsignmentNoSuccessArgs> ConsignmentNoSuccess; //valid response indicating Consignment not booked
        public event EventHandler<ErrorArgs> DocumentFailedToLoad;

        //constants
        public const string TARGET_URL = "https://express.tnt.com/expressconnect/shipping/ship";
        public const string ShipRequestSchemaUri = @".\TNTConnector\xsd\ShipmentRequestIN.xsd";

        //debug constants
        public readonly DateTime? ReplaceShipDate = null;                       //null for production

        //instance variables
        public Statusses Status { get; private set; }
        private HttpClient client;
        private bool Debug;    //(1) Live / test TNT account (2)TODO: log all communication?

        //current ShipRequest including results and follow-up
        private TNTShipRequest shipReq;//the initial request
        TNTPackageAnalyser pa;
        private string accessCode;     //received as answer to initial request, necessary for getting TNTResult and other docs
        private TNTResult shipRes;
        private string bookingRef;     //bookingRef for the whole batch
        private Dictionary<DocTypes, DocStatusses> docStatus;
        private IEnumerator<DocTypes> docIndex;  //where are we in getting the docs?
        public PrintHTMLQueue printQueue { get; private set; }       //print queue for the docs

        /// <summary>
        /// Constructor: configure a httpclient
        /// </summary>
        public TNTConnectorShipRequest(bool Debug)
        {
            this.Debug = Debug;
            this.client = new HttpClient();
            this.client.Timeout = new TimeSpan(0, 0, 30);
            this.pa = new TNTPackageAnalyser();
            this.printQueue = new PrintHTMLQueue();
        }

        public void Dispose()
        {
            this.client.Dispose();
            this.printQueue.Dispose();
        }

        /// <summary>
        /// Start building a new TNTShipRequest
        /// </summary>
        /// <param name="CollectionDate">Date at which to book a collection</param>
        /// <param name="Type">Full or "PrintOnly"</param>
        public void NewShipRequest(DateTime CollectionDate, TNTShipRequest.RequestTypes Type)
        {
            if (ReplaceShipDate != null)
            {
                CollectionDate = (DateTime)ReplaceShipDate;
            }
            this.shipReq = new TNTShipRequest(CollectionDate, Type, this.pa, Debug);
            this.Status = Statusses.BuildRequest;
            this.accessCode = "";
            //this.conRefs = new List<string>();
            this.bookingRef = "";
            this.docStatus = new Dictionary<DocTypes, DocStatusses>()
            {
                { DocTypes.Label, DocStatusses.Initial },
                { DocTypes.Manifest, DocStatusses.Initial },
                { DocTypes.Connote, DocStatusses.Initial },
                { DocTypes.Invoice, DocStatusses.Initial },
            };
            this.docIndex = new DocTypesEnum().GetEnumerator();
        }

        /// <summary>
        /// Add a consignment to the request
        /// </summary>
        /// <param name="o">The Sales Order to send</param>
        public void AddConsignment(SalesOrder o)
        {
            bool conIDNull = (o.PackageTrackingNo == null || o.PackageTrackingNo == "");
            if (
                shipReq.reqestType == TNTShipRequest.RequestTypes.Full && conIDNull 
                || shipReq.reqestType == TNTShipRequest.RequestTypes.PrintOnly && !conIDNull
                )
            {
                shipReq.AddConsignment(o);
                //conRefs.Add(o.SalesOrderId);
            }
            else
            {
                throw new InvalidOperationException("TNTConnector error: requestType=" + shipReq.reqestType + ", but Package Tracking Number was " + (conIDNull ? " not given." : o.PackageTrackingNo));
            }
        }

        /// <summary>
        /// Call it after all consignments have been added.
        /// A chain of async proccesses is started. Listen to the events to know what's happening.
        /// </summary>
        public async Task StartSendingAsync()
        {
            await initialRequestAsync();
        }

        //private methods to execute the various sub-requests

        /// <summary>
        /// Send the Consignment Request document to TNT and receive an Accesscode as a response
        /// </summary>
        private async Task initialRequestAsync()
        {
            this.Status = Statusses.SendRequest;
            string errors = shipReq.Validate(ShipRequestSchemaUri);
            if (errors.Length > 0)
            {
                throw new InvalidOperationException("Schema validation error(s): \r\n" + errors);
            }
            HttpContent content = BuildWWWFormUrlEncoded(shipReq.xml);
            PostResults res = await PostAsync(content, expectXml: false);

            //Check that we received the access code and start the next request
            string answer = res.Content;
            if (answer.StartsWith("COMPLETE:") && answer.Length >= 17)
            {
                this.accessCode = answer.Substring(9);
                FireAccesscodeReceived();
                //start next request
                await GetResult();
            }
            else
            {
                FireShipRequestError(0, "No Accesscode Received, Response was: " + answer);
            }
        }

        /// <summary>
        /// Use the Accesscode to get the TNTResult document that will show us if the consignments were booked
        /// </summary>
        private async Task GetResult()
        {
            this.Status = Statusses.GetResult;
            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "xml_in", "GET_RESULT:" + this.accessCode }  //application/x-www-form-urlencoded like it's meant to be... no xml really!
            });
            PostResults resp = await PostAsync(content, expectXml: true);

            //this time the result is xml and we go analyse it
            TNTResult.IndexBy indexBy = (this.shipReq.reqestType == TNTShipRequest.RequestTypes.Full) ?
                TNTResult.IndexBy.CONREF : TNTResult.IndexBy.CONNUMBER;
            this.shipRes = new TNTResult(resp.Xml, indexBy);

            if (shipRes.OK && !shipRes.hasAnyError)
            {
                if (shipReq.reqestType == TNTShipRequest.RequestTypes.Full && shipRes.hasShip)
                {
                    foreach (var c in shipRes) //iterate the consignments
                    {
                        if (shipRes.Success) //consignment booked?
                        {
                            FireConsignmentSuccess(shipRes.ConRef, shipRes.ConNumber);
                        }
                        else
                        {
                            FireConsignmentNoSuccess(shipRes.ConRef);
                        }
                    }
                }
                else
                { 
                    if (shipReq.reqestType == TNTShipRequest.RequestTypes.PrintOnly && !shipRes.hasShip)
                    {
                        //good! just proceed
                    }
                    else //surely wrong
                    {
                        if (shipReq.reqestType == TNTShipRequest.RequestTypes.Full && !shipRes.hasShip)
                        {
                            FireShipResultError(0, "Error in result document: full request, but SHIP element not found in result.");
                        }
                        else
                        {
                            FireShipResultError(0, "Error in result document: request was 'printonly' and still SHIP is present.");
                        }
                        return;  //no follow up after these errors
                    }
                }
                //Path of (at least partial) success: which docs do we need to get from te server?
                if (shipRes.IsLabelCreated) this.docStatus[DocTypes.Label] = DocStatusses.OnTNTServer;
                if (shipRes.IsManifestCreated) this.docStatus[DocTypes.Manifest] = DocStatusses.OnTNTServer;
                if (shipRes.IsConnoteCreated) this.docStatus[DocTypes.Connote] = DocStatusses.OnTNTServer;
                if (shipRes.IsInvoiceCreated) this.docStatus[DocTypes.Invoice] = DocStatusses.OnTNTServer;
                //event
                FireResultReceived(docStatus[DocTypes.Label],
                    docStatus[DocTypes.Manifest],
                    docStatus[DocTypes.Connote],
                    docStatus[DocTypes.Invoice]
                );
                //start follow up requests!
                this.Status = Statusses.GetDocs;
                await GetNextExpectedDocument();
            }
            else
            {
                FireShipResultError(0, "Error in result document: " + shipRes.GetFirstErrorDescr());
            }
        }

        /// <summary>
        /// Iterate the docTypes (Label, Manifest, ...), until you find one that needs fetching
        /// This method calls itself indirectly, until there is nothing more to fetch.
        /// </summary>
        private async Task GetNextExpectedDocument()
        {
            bool requestStarted = false;
            bool moveSucceeded = true;
            moveSucceeded = docIndex.MoveNext();
            while (moveSucceeded && !requestStarted)
            {
                DocTypes type = docIndex.Current;
                if (docStatus[type] == DocStatusses.OnTNTServer)
                {
                    requestStarted = true;
                    await GetDocument(type);
                }
                else
                {
                    moveSucceeded = docIndex.MoveNext();
                }
            }
            if (!moveSucceeded) //no more docTypes, it means we're ready
            {
                this.Status = Statusses.Ready;
                FireShipRequestReady();
            }
        }

        /// <summary>
        /// Fetch one of the 4 printable documents, as readonly, from the TNT server, apply an XSLT stylesheet,
        /// then send it to a printer (last point still TODO)
        /// </summary>
        /// <param name="type">One of the four document types</param>
        private async Task GetDocument(DocTypes type)
        {
            docStatus[type] = DocStatusses.Fetching;
            FireDocumentRequest(type);

            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "xml_in", "GET_" + type.ToString().ToUpper() + ":" + this.accessCode }
            });
            PostResults resp = await PostAsync(content, expectXml: true);

            //Choose the stylsheet fitting to the doc type
            XslCompiledTransform xsl = new XslCompiledTransform();
            switch (type)
            {
                case DocTypes.Label:
                    xsl.Load(typeof(AddressLabelXsl));
                    break;
                case DocTypes.Manifest:
                    xsl.Load(typeof(ManifestXsl));
                    break;
                case DocTypes.Connote:
                    xsl.Load(typeof(ConsignmentNoteXsl));
                    break;
                case DocTypes.Invoice:
                    xsl.Load(typeof(CommercialInvoiceXsl));
                    break;
            }

            //Transform the response, with the stylesheet, then layout and print
            TextWriter wri = new StringWriter();
            XsltArgumentList args = new XsltArgumentList();
            xsl.Transform(resp.Xml.docR, args, wri);
            bool loadingProblems = printQueue.AddPrintJob(wri.ToString() );  //Load into browser, then print
            if (loadingProblems)
                FireDocumentLoadError(0, "One of the HTML documents to be printed did not load into the .NET browser properly.");

            docStatus[type] = DocStatusses.Fetched;
            FireDocumentReceived(type);
            await GetNextExpectedDocument();  //the recursive call will end after max 4 iterations (the docTypes).
        }

        /// <summary>
        /// Checks for unpleasant HTTP status codes as well as other problems with the response.
        /// All errors found here are fatal: they break the chain of async processes.
        /// </summary>
        /// <param name="content">HttpContent for Posting to TNT</param>
        /// <param name="expectXml">Do we expect the response to be XML?</param>
        private async Task<PostResults> PostAsync(HttpContent content, bool expectXml)
        {
            HttpResponseMessage resp = null;
            try
            {
                resp = await client.PostAsync(new Uri(TARGET_URL), content);
            }
            catch (Exception e)
            {
                FirePostError(0, "PostAsync Exception: " + e.Message);
                throw e;  //just to stop the flow
            }
            if (!resp.IsSuccessStatusCode)
            {
                FirePostError(0, "HTTP error status " + resp.StatusCode + ": " + resp.ReasonPhrase);
                throw new InvalidOperationException("HTTP error status " + resp.StatusCode + ": " + resp.ReasonPhrase);
            }
            //xml checks
            if (expectXml)
            {
                MyXMLDocument xdoc = null;
                Stream st = await resp.Content.ReadAsStreamAsync();
                try
                {
                    TextReader rd = new StreamReader(st, new UTF8Encoding(false, true));
                    XmlReader XReader = XmlReader.Create(rd);
                    xdoc = new MyXMLDocument(XReader, readOnly: true);
                    if (
                        xdoc.NodeExists("/runtime_error") ||
                        xdoc.NodeExists("/parse_error")
                        )
                    {
                        string errormsg = xdoc.GetStringValue("//error_reason/text()");
                        FirePostError(0, "Errormessage sent by TNT: " + errormsg);
                        throw new InvalidOperationException("Errormessage sent by TNT: " + errormsg);
                    }
                }
                catch (Exception e)
                {
                    FirePostError(0, "Problem parsing xml " + e.Message);
                    throw e;
                }
                return new PostResults(null, xdoc);
            }
            else
            {
                string c = await resp.Content.ReadAsStringAsync();
                return new PostResults(c, null);
            }
        }

        /// <summary>
        /// Returnvalue for the previous method
        /// </summary>
        private class PostResults
        {
            public PostResults(string Content, MyXMLDocument Xml)
            {
                this.Content = Content;
                this.Xml = Xml;
            }
            public string Content { get; private set; }
            public MyXMLDocument Xml { get; private set; }
        }

        /// <summary>
        /// Transform an xml-document to a HttpContent request message that accords to the TNT spec.
        /// It has to be application/x-www-form-urlencoded !
        /// It has to contain one key-value pair, "xml_in" being the key and the whole xml-string the value (urlencoded of course).
        /// The whole thing should be in UTF-8.
        /// </summary>
        /// <param name="xml">Xmldocument to send</param>
        private HttpContent BuildWWWFormUrlEncoded(MyXMLDocument xml)
        {
            Byte[] bytes;
            //First we get the xml to a stream... to make the XmlWriter believe it is boss of encoding (UTF-8)
            using (MemoryStream ms = new MemoryStream())
            {
                xml.ToStream(ms, new UTF8Encoding(false, true), indent: true);
                bytes = ms.ToArray();  //not exactly the streaming + async philosophy, but we have to do some strange stuff
            }
            //Then we get the stream into a bytes array to enable us to do URL-encoding
            Byte[] bytes2 = HttpUtility.UrlEncodeToBytes(bytes);
            //We create an un-encoded namevaluepair, with bytes2 als the value
            string prolog = "xml_in=";
            Byte[] bytes3 = Encoding.UTF8.GetBytes(prolog);
            //Concatenate: bytes3 + bytes2
            Byte[] bytes4 = new byte[bytes3.Length + bytes2.Length];
            Buffer.BlockCopy(bytes3, 0, bytes4, 0, bytes3.Length);
            Buffer.BlockCopy(bytes2, 0, bytes4, bytes3.Length, bytes2.Length);
            //...all of this because we had to do URLEncoding at an unusual time, after writing the xml
            HttpContent content = new ByteArrayContent(bytes4);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
        }

        /// <summary>
        /// Make the DocTypes enum truly Enumerable
        /// </summary>
        private class DocTypesEnum : IEnumerable<DocTypes>
        {
            public IEnumerator<DocTypes> GetEnumerator()
            {
                foreach (DocTypes t in Enum.GetValues(typeof(DocTypes)) )
                {
                    yield return t;
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        //private "FireEvent" methods for each of the events, plus public Custom EventArgs classes if needed

        private void FireAccesscodeReceived()
        {
            EventHandler handler = AccessCodeReceived;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void FireResultReceived(DocStatusses LabelStatus, DocStatusses ManifestStatus, DocStatusses ConnoteStatus, DocStatusses InvoiceStatus)
        {
            EventHandler<ResultReceivedArgs> handler2 = ResultReceived;
            if (handler2 != null)
            {
                handler2(this, new ResultReceivedArgs(LabelStatus, ManifestStatus, ConnoteStatus, InvoiceStatus));
            }
        }

        public class ResultReceivedArgs : EventArgs
        {
            public ResultReceivedArgs (DocStatusses LabelStatus, DocStatusses ManifestStatus, DocStatusses ConnoteStatus, DocStatusses InvoiceStatus)
            {
                this.LabelStatus = LabelStatus;
                this.ManifestStatus = ManifestStatus;
                this.ConnoteStatus = ConnoteStatus;
                this.InvoiceStatus = InvoiceStatus;
            }
            public DocStatusses LabelStatus { get; private set; }
            public DocStatusses ManifestStatus { get; private set; }
            public DocStatusses ConnoteStatus { get; private set; }
            public DocStatusses InvoiceStatus { get; private set; }
        }

        private void FireConsignmentSuccess(string ConRef, string ConNumber)
        {
            EventHandler<ConsignmentSuccessArgs> handler = ConsignmentSuccess;
            if (handler != null)
            {
                handler(this, new ConsignmentSuccessArgs(ConRef, ConNumber));
            }
        }

        public class ConsignmentSuccessArgs : EventArgs
        {
            public ConsignmentSuccessArgs(string ConRef, string ConNumber)
            {
                this.ConRef = ConRef;
                this.ConNumber = ConNumber;
            }
            public string ConRef { get; private set; }
            public string ConNumber { get; private set; }
        }

        private void FireDocumentRequest(DocTypes DocType)
        {
            EventHandler<DocEventArgs> handler = DocumentRequest;
            if (handler != null)
            {
                handler(this, new DocEventArgs(DocType));
            }
        }

        private void FireDocumentReceived(DocTypes DocType)
        {
            EventHandler<DocEventArgs> handler2 = DocumentReceived;
            if (handler2 != null)
            {
                handler2(this, new DocEventArgs(DocType));
            }
        }

        public class DocEventArgs : EventArgs
        {
            public DocEventArgs(DocTypes DocType)
            {
                this.DocType = DocType;
            }
            public DocTypes DocType { get; private set; }
        }

        private void FireShipRequestReady()
        {
            EventHandler handler = ShipRequestReady;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        //Error events

        private void FirePostError(int ErrorNumber, string Description)
        {
            EventHandler<ErrorArgs> handler = PostError;
            if (handler != null)
            {
                handler(this, new ErrorArgs(ErrorNumber, Description));
            }
        }

        private void FireShipRequestError(int ErrorNumber, string Description)
        {
            EventHandler<ErrorArgs> handler = ShipRequestError;
            if (handler != null)
            {
                handler(this, new ErrorArgs(ErrorNumber, Description));
            }
        }

        private void FireShipResultError(int ErrorNumber, string Description)
        {
            EventHandler<ErrorArgs> handler = ShipResultError;
            if (handler != null)
            {
                handler(this, new ErrorArgs(ErrorNumber, Description));
            }
        }

        private void FireDocumentLoadError(int ErrorNumber, string Description)
        {
            EventHandler<ErrorArgs> handler = DocumentFailedToLoad;
            if (handler != null)
            {
                handler(this, new ErrorArgs(ErrorNumber, Description));
            }
        }

        public class ErrorArgs : EventArgs
        {
            public ErrorArgs(int ErrorNumber, string Description)
            {
                this.ErrorNumber = ErrorNumber;
                this.Description = Description;
            }
            public int ErrorNumber { get; private set; }
            public string Description { get; private set; }
        }

        public class DocErrorArgs : EventArgs
        {
            public DocErrorArgs(DocTypes DocType, int ErrorNumber, string Description)
            {
                this.DocType = DocType;
                this.ErrorNumber = ErrorNumber;
                this.Description = Description;
            }
            public DocTypes DocType { get; private set; }
            public int ErrorNumber { get; private set; }
            public string Description { get; private set; }
        }

        private void FireConsignmentNoSuccess(string ConRef)
        {
            EventHandler<ConsignmentNoSuccessArgs> handler = ConsignmentNoSuccess;
            if (handler != null)
            {
                handler(this, new ConsignmentNoSuccessArgs(ConRef));
            }
        }

        public class ConsignmentNoSuccessArgs : EventArgs
        {
            public ConsignmentNoSuccessArgs(string ConRef)
            {
                this.ConRef = ConRef;
            }
            public string ConRef { get; private set; }
        }
    }
}
