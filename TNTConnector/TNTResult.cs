using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// TNT Result document
    /// It is the answer to a ConsignmentRequest document, which may create, book, ship and/or print one or more Consignments.
    /// A Result document states for each Consigment whether the activity (create, book, ship) was a SUCCESS.
    /// It also indicates which additional documents are available for printing ("print" activity) and can be fetched from the TNT server.
    /// 
    /// In the document, Consigments are identified by either a CONREF or CONNUMBER. CONREF is a relative identifier, unique only within 
    /// this document, and also within the originating Request: it is a number that we assigned. A CONNUMBER is unique within TNT, 
    /// and is assigned by them.
    /// If this Result is the answer to a request that just created the Consignments, then CONREF identifies the Consignents.
    /// If this Result is the answer to a follow-up request then CONNUMBER is best used to identify the Consignments.
    /// Please indicate to the constructor which of the two you want!
    /// 
    /// This object is IEnumerable to enable iterating over the Consignments.
    /// The enumerator returns ids that are either CONREFs or CONNUMBERs (whichever you indicated to the constructor).
    /// But apart from that, the enumerator scrolls this whole object to the next Consigment and makes its properties available for 
    /// getting.
    /// </summary>
    public class TNTResult : IEnumerable<string>
    {
        /// <summary>
        /// Two ways of indexing this document
        /// </summary>
        public enum IndexBy { CONREF, CONNUMBER };

        /// <summary>
        /// Is the document worthy of further investigation? Or is it just an error message.
        /// </summary>
        public bool OK { get; private set; } 

        //'What sections do we have in the document?
        public bool hasCreate { get; private set; }
        public bool hasBook { get; private set; }
        public bool hasShip { get; private set; }
        //...activity 'rate' may be added
        public bool hasPrint { get; private set; }
        public bool hasError { get; private set; }
        public bool hasRuntimeError { get; private set; }
        public bool hasAnyError { get { return (hasError || hasRuntimeError); } }

        private enum DocTypes { CONNOTE, LABEL, MANIFEST, INVOICE };
        private enum ConsignmentProps { CONREF, CONNUMBER, SUCCESS };

        private MyXMLDocument doc;

        //Iterator for the 'create' consignments: to find conRefs or conNumbers from the document
        private MyXMLChildEnumerable createEnum;

        //Indexed property name: either "/CONREF" or "/CONNUMBER", xpath relative to CREATE element
        private XPathExpression IdPropertyXPath;

        //Indexes for the various activities, with either ConRef or ConNumber as key (chosen at construction)
        private MyXMLChildIndex createIndex;
        private MyXMLChildIndex bookIndex;
        private MyXMLChildIndex shipIndex;
        //...actvity 'rate' may need its' own index

        //Data relating to the current consignment
        private MyXMLDocument curCreate;
        private MyXMLDocument curBook;
        private MyXMLDocument curShip;
        //...other activities...

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">MyXMLDocument that contains the TNT Result, XML already parsed</param>
        /// <param name="indexBy">Shall we index the document by ConRef or by ConNumber?</param>
        public TNTResult(MyXMLDocument doc, IndexBy indexBy)
        {
            this.doc = doc;
            this.OK = Initialize(indexBy);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">XmlReader that contains the TNT result</param>
        /// <param name="indexBy">Shall we index the document by ConRef or by ConNumber?</param>
        public TNTResult(XmlReader reader, IndexBy indexBy)
        {
            this.doc = new MyXMLDocument(reader, readOnly: true);
            this.OK = Initialize(indexBy);
        }

        /// <summary>
        /// Make the Consignments enumerable according to the CREATE section of the doc.
        /// Then index the Consignments for each of the relevant sections.
        /// Assumption: The CREATE section exists, or we have no Consignments.
        /// </summary>
        private bool Initialize(IndexBy indexBy)
        {
            IdPropertyXPath = XPathExpression.Compile("./" + indexBy.ToString() + "/text()");
            //determine available sections
            hasCreate = doc.NodeExists("/document/CREATE");
            hasBook =   doc.NodeExists("/document/BOOK");
            hasShip =   doc.NodeExists("/document/SHIP");
            hasPrint =  doc.NodeExists("/document/PRINT");
            hasError =  doc.NodeExists("//ERROR");
            hasRuntimeError = doc.NodeExists("/runtime_error");
            //prepare iterator and indexes
            if (doc.NodeExists("/document") )
            {
                if (hasCreate)
                {
                    createEnum = doc.GetChildEnumerable(XPathExpression.Compile("/document"), "CREATE");
                    createIndex = doc.GetChildIndex(XPathExpression.Compile("/document"), "CREATE", IdPropertyXPath);
                }
                if (hasBook)
                {
                    bookIndex = doc.GetChildIndex(XPathExpression.Compile("/document/BOOK"), "CONSIGNMENT", IdPropertyXPath);
                }
                if (hasShip)
                {
                    shipIndex = doc.GetChildIndex(XPathExpression.Compile("/document/SHIP"), "CONSIGNMENT", IdPropertyXPath);
                }
                //...etc
                return true;
            }
            else return false; //after all, not worthy of further investigation.
        }

        /// <summary>
        /// Lookup a Consignment by Id and make it current: ready for further reading.
        /// This one has a weaker assumption: either CREATE or SHIP must be there to have Consignments
        /// </summary>
        /// <param name="Id">Either a ConRef or a ConNumber, depending on the type of Id chosen, see constructor</param>
        /// <returns>success?</returns>
        public bool FindConsignmentById(string Id)
        {
            if (!hasCreate && !hasShip)
            {
                throw new InvalidOperationException("TNTResult: no CREATE and no SHIP element, therefore no consignment info.");
            }
            bool OK = true;
            if (hasCreate)
            {
                curCreate = createIndex.GetChildByIndexValue(Id);
                OK = OK && (curCreate != null);
            }
            if (hasBook)
            {
                curBook = bookIndex.GetChildByIndexValue(Id);
                OK = OK && (curBook != null);
            }
            if (hasShip)
            {
                curShip = shipIndex.GetChildByIndexValue(Id);
                OK = OK && (curShip != null);
            }
            return OK;
        }

        /// <summary>
        /// Iterate over the available consignments and return their ids.
        /// As a side effect this whole document is scrolled to make the corresponding consignment available for reading.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            if (hasCreate) //following the assumption: no CREATE section? then no Consignments
            {
                foreach (MyXMLDocument idoc in createEnum)
                {
                    string Id = idoc.GetStringValue(IdPropertyXPath);
                    bool OK = FindConsignmentById(Id);
                    if (!OK)
                    {
                        throw new InvalidOperationException("TNTResult: scrolling to the next Consignment failed unexpectedly.");
                    }
                    yield return Id;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //Methods to read the current Consigment from the doc

        /// <summary>
        /// Get a Consignment property from either CREATE or SHIP
        /// Note: We could even require that they are equal in both, or we could require SHIP and disregard CREATE
        /// </summary>
        /// <param name="prop">the property to get</param>
        private string GetConProp(ConsignmentProps prop)
        {
            if (hasShip)
            {
                return curShip.GetStringValue("./" + prop.ToString() + "/text()");
            }
            else if (hasCreate)
            {
                return curCreate.GetStringValue("./" + prop.ToString() + "/text()");
            }
            else return null;
        }

        /// <summary>
        /// What consignment are we talking about?: ConRef is a relative identifier, unique only within the request
        /// It is an identifier that we assign
        /// </summary>
        public string ConRef { get { return GetConProp(ConsignmentProps.CONREF); } }

        /// <summary>
        /// What consignment are we talking about?: ConNumber should be unique within TNT
        /// We don't know it beforehand, we get it returned in both the CREATE and SHIP elements
        /// </summary>
        public string ConNumber { get { return GetConProp(ConsignmentProps.CONNUMBER); } }

        /// <summary>
        /// Was the action (preferably SHIP) for this consignment a success?
        /// SHIP can't succeed if CREATE didn't so it makes sense to look at SHIP only, if it's there
        /// </summary>
        public bool Success { get { return (GetConProp(ConsignmentProps.SUCCESS) == "Y"); } }

        //Properties of the current consignment that are specific to the BOOK activity
        //The BOOK activity is not in use currently! These 3 methods are at present untested.

        private bool IsBookSuccess { get
            {
                if (hasBook)
                {
                    return (curBook.GetStringValue("./SUCCESS/text()") == "Y");
                }
                else return false;
            } }

        /// <summary>
        /// Returns the BookingRef, only if the booking was a success (null otherwise)
        /// </summary>
        public string BookingRef { get
            {
                if (IsBookSuccess)
                {
                    return curBook.GetStringValue("./BOOKINGREF/text()");
                }
                else return null;
            } }

        /// <summary>
        /// Is TNT telling us we are a FIRSTTIMETRADER ?
        /// </summary>
        public bool? IsFirstTimeTrader { get
            {
                if (IsBookSuccess)
                {
                    return (curBook.GetStringValue("./FIRSTTIMETRADER/text()") == "Y");
                }
                else throw new InvalidOperationException("FirstTimeTrader: There is no BOOK element, so FIRSTIMETRADER is not applicable.");
            } }

        //Which documents are available on the TNT server, according to this Result doc?

        private bool IsPrintCreated(DocTypes type)
        {
            bool created = false;
            if (hasPrint)
            {
                string xpath = "/document/PRINT/" + type.ToString() + "/text()";
                var node = doc.nav.SelectSingleNode(xpath);
                if (node != null)
                {
                    created = node.Value == "CREATED";
                }
            }
            return created;
        }

        public bool IsConnoteCreated { get { return IsPrintCreated(DocTypes.CONNOTE); } }
        public bool IsLabelCreated { get { return IsPrintCreated(DocTypes.LABEL); } }
        public bool IsManifestCreated { get { return IsPrintCreated(DocTypes.MANIFEST); } }
        public bool IsInvoiceCreated { get { return IsPrintCreated(DocTypes.INVOICE); } }

        //Other general information

        /// <summary>
        /// Read the first error description from the document, or the "error_srcText" in case of a "runtime_error" document
        /// </summary>
        public string GetFirstErrorDescr()
        {
            if (hasError)
            {
                return doc.GetStringValue("//ERROR/DESCRIPTION/text()");
            }
            else if (hasRuntimeError)
            {
                return doc.GetStringValue("/runtime_error/error_srcText/text()");
            }
            else return null;
        }
    }
}
