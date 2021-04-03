using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// The class is named after the TNT "ExpressConnect Shipping" module that has 1 single request format
    /// for 4 different activities (create, rate, book, ship, print) alle applicable to 1 or more consignments
    /// 
    /// We prepare to do two kinds of requests from the many that are possible:
    /// (1) "Full" includes the Create, Ship and Print activities. It is enough to send packages if you have arranged
    /// collection times separately with TNT.
    /// (2) "PrintOnly" includes only the Print activity. Use it to re-generate printable documents for Consignments
    /// that were earlier already succesfully Created + "Ship"-ed on the TNT-server. (but probably not shipped for real)
    /// 
    /// This class can write itself to an XML "Ship request" document
    /// </summary>
    public class TNTShipRequest
    {
        /// <summary>
        /// The two kinds of ShipRequests that this connector can handle.
        /// </summary>
        public enum RequestTypes { Full, PrintOnly };

        //logins for testing
        private const string LoginCompanyT = "NTOCARE_T";
        private const string LoginPasswordT = "tnt12345T";

        //live logins
        private const string LoginCompany = "NTOCARE";
        private const string LoginPassword = "tnt12345";  //test of je live logins gebruikt: hier een 9 achter zetten, dan geeft TNT login fout - gebruik zending uit verleden

        //sender data
        public const string SenderCompanyname = "ENTOCARE C.V.";
        public const string SenderStreetAddress1 = "HAAGSTEEG 4";
        public const string SenderCity = "WAGENINGEN";
        public const string SenderPostalcode = "6708 PM";
        public const string SenderCountry = "NL";
        public const string SenderTNTAccount = "151993";
        public const string SenderContactName = "Maedeli Hennekam";
        public const string SenderContactTelephone = "+31317411188";
        public const string SenderContactEmail = "post@entocare.nl";

        //collect time
        public const string PrefCollecttimeFromDbg = "14:55";
        public const string PrefCollecttimeFromLiv = "16:00";
        public const string PrefCollecttimeTo = "18:00";    //We willen tussen 16:00 en 18:00, maar hij moppert als niet overlapt met 15:00

        //other standard texts
        public const string Deliveryinst_HoldForPickup = "Hold for pickup";
        public const string ArticleDescription = "beneficials";
        public const string ArticleHts = "01069000";
        public const string PackageDescription = "package";

        //other constants
        public const string LoginAppversion = "2.2";
        public const string ConType1 = "N";             //Consigment type: always Non-documents
        public const string PaymentInd1 = "S";          //Sender pays. Alternative: 'R' receiver pays requires permission and elaborate setup
        public const string ShowBookingRef = "Y";       //Do we want the bookingRef number in the result?
        public const string BookingEmail = "Y";         //Confirmation email to SENDER
        public const string DateFormat = @"dd/MM/yyyy"; //used in SHIPDATE element, TNT rule differs from ING/SEPA! Backslash needed so "/" is taken literally

        public RequestTypes reqestType { get; private set; }
        private bool Debug;
        private TNTPackageAnalyser pa;
        public MyXMLDocument xml { get; private set; }  //the wrapped request, ready for validation and conversion to string /bytestream
        //private StringWriter validationerrors;
        private DateTimeFormatInfo dateformatinfo;

        public TNTShipRequest(DateTime CollectionDate, RequestTypes requestType, TNTPackageAnalyser pa, bool Debug = false)
        {
            this.reqestType = requestType;
            this.pa = pa;
            this.Debug = Debug;
            this.xml = new MyXMLDocument();
            dateformatinfo = new DateTimeFormatInfo();  //neutral culture, it has / as date separator
            MakeFixedElements(CollectionDate);
        }

        public void AddConsignment(SalesOrder o)
        {
            if (reqestType == RequestTypes.Full)
            {
                string ConRef = o.SalesOrderId;
                //xml.NavigateThis("/ESHIPPER/CONSIGNMENTBATCH") -- do it in MakeConsignment;
                MakeConsignment(o, ConRef);

                xml.NavigateThis("/ESHIPPER/ACTIVITY");
                xml.AddNewElt("./CREATE", "CONREF", ConRef, 20);
                xml.AddNewElt("./BOOK", "CONREF", ConRef, 20);
                xml.AddNewElt("./SHIP", "CONREF", ConRef, 20);
                xml.AddNewElt("./PRINT/REQUIRED", "CONREF", ConRef, 20);
            }
            else //PrintOnly requestType
            {
                string ConNumber = o.PackageTrackingNo; //CREATE has been done already, so we need ConNumbers now
                xml.NavigateThis("/ESHIPPER/ACTIVITY");
                xml.AddNewElt("./PRINT/REQUIRED", "CONNUMBER", ConNumber, 15);
            }
        }

        /// <summary>
        /// Create the fixed elements of the shipRequest message and fill them with (mostly) constant data, plus the Collection Date
        /// </summary>
        /// <param name="CollectionDate">the Collection Date</param>
        private void MakeFixedElements(DateTime CollectionDate)
        {
            xml.AddNewElt("/", "ESHIPPER");

            xml.AddNewElt("/ESHIPPER", "LOGIN");
            xml.NavigateThis("/ESHIPPER/LOGIN");
            xml.AddNewElt(".", "COMPANY", (Debug ? LoginCompanyT : LoginCompany), 99);
            xml.AddNewElt(".", "PASSWORD", (Debug ? LoginPasswordT : LoginPassword), 99);
            xml.AddNewElt(".", "APPID", "IN", 99);
            xml.AddNewElt(".", "APPVERSION", LoginAppversion, 99);

            xml.AddNewElt("/ESHIPPER", "CONSIGNMENTBATCH");
            xml.AddNewElt("/ESHIPPER/CONSIGNMENTBATCH", "SENDER");
            xml.NavigateThis("/ESHIPPER/CONSIGNMENTBATCH/SENDER");
            this.AddAddress(SenderCompanyname, SenderStreetAddress1, "", "", SenderCity, "", SenderPostalcode, SenderCountry,
                SenderTNTAccount, "", SenderContactName, "000", SenderContactTelephone, SenderContactEmail);
            xml.AddNewElt(".", "COLLECTION");
            xml.NavigateThis("./COLLECTION");
            xml.AddNewElt(".", "SHIPDATE", TNTDate(CollectionDate), 12);
            xml.AddNewElt(".", "PREFCOLLECTTIME");
            xml.AddNewElt("./PREFCOLLECTTIME", "FROM", Debug ? PrefCollecttimeFromDbg : PrefCollecttimeFromLiv, 5);
            xml.AddNewElt("./PREFCOLLECTTIME", "TO", PrefCollecttimeTo, 5);
            xml.AddNewElt(".", "COLLINSTRUCTIONS", "", 24);
            //Note: just before the end of the CONSIGNMENTBATCH, one or more CONSIGNMENT elements will be added later

            xml.AddNewElt("/ESHIPPER", "ACTIVITY");
            xml.NavigateThis("/ESHIPPER/ACTIVITY");
            //Note: each of the following elements CREATE, BOOK, SHIP and PRINT will get a CONREF or CONNUMBER child later
            //   for each of the consignments in the batch.
            if (reqestType == RequestTypes.Full)
            {
                xml.AddNewElt(".", "CREATE");
                xml.AddNewElt(".", "BOOK");
                xml.AddNewAttr("./BOOK", "ShowBookingRef", ShowBookingRef, 1);
                xml.AddNewAttr("./BOOK", "EMAILREQD", BookingEmail, 1);
                xml.AddNewElt(".", "SHIP");
            }
            xml.AddNewElt(".", "PRINT");
            xml.NavigateThis("./PRINT");
            xml.AddNewElt(".", "REQUIRED");
            xml.AddNewElt(".", "EMAILTO", SenderContactEmail, 99);
            xml.AddNewElt(".", "EMAILFROM", SenderContactEmail, 99);
        }

        private void MakeConsignment(SalesOrder o, string Conref)
        {
            xml.NavigateThis("/ESHIPPER/CONSIGNMENTBATCH");
            xml.AddNewElt(".", "CONSIGNMENT");
            xml.MoveToLastChild();  //Move to the just created CONSIGNMENT!
            xml.AddNewElt(".", "CONREF", Conref, 20);
            xml.AddNewElt(".", "DETAILS");
            xml.AddNewElt("./DETAILS", "RECEIVER");
            xml.NavigateThis("./DETAILS/RECEIVER");
            AddAddress(o.CompanyName, o.StreetAddress1, o.StreetAddress2, o.StreetAddress3, o.City, o.Province, o.PostCode, o.CountryAbb,
                null, o.Vat, o.ContactName, o.ContactDialCode, o.ContactTelephone, o.ContactEmail);
            //back to: "DETAILS"
            xml.NavigateThis("..");  
            xml.AddNewElt(".", "CUSTOMERREF", o.SalesOrderNumber, 24);
            xml.AddNewElt(".", "CONTYPE", "N", 1);     //D=Document, N=Non-document
            xml.AddNewElt(".", "PAYMENTIND", "S", 1);  //S=Sender pays, R=Receiver pays (only possible after negotiation)

            double invoiceValue = (!o.inEU) ? o.InvoiceValue : 0.01;  //0 is no longer accepted
            pa.MakePackages(o.PackageFormatCode, !o.inEU, invoiceValue, xml);
            xml.AddNewElt(".", "ITEMS", pa.TotalItems.ToString(), 3);
            xml.AddNewElt(".", "TOTALWEIGHT", pa.TotalWeight, 8, 3);
            xml.AddNewElt(".", "TOTALVOLUME", pa.TotalVolume, 7, 3);
            xml.AddNewElt(".", "CURRENCY", "EUR", 3);
            xml.AddNewElt(".", "GOODSVALUE", invoiceValue, 15, 2);
            xml.AddNewElt(".", "INSURANCEVALUE", 0.01, 15, 2);  //a number is required, we used to leave this empty!
            xml.AddNewElt(".", "INSURANCECURRENCY", "", 3);     //like above
            xml.AddNewElt(".", "SERVICE", o.ShippingAgentService, 3);
            xml.AddNewElt(".", "OPTION", "", 3);
            xml.AddNewElt(".", "DESCRIPTION", ArticleDescription, 90);
            xml.AddNewElt(".", "DELIVERYINST", o.DeliveryInstructions, 60);
            foreach (TNTPackageAnalyser.PackageMultiple pack in pa)
            {
                xml.AddNewElt(".", "PACKAGE");
                xml.MoveToLastChild();  //lastChild should be the newly added PACKAGE
                xml.AddNewElt(".", "ITEMS", pack.HowMany.ToString(), 4);
                xml.AddNewElt(".", "DESCRIPTION", PackageDescription, 60);
                xml.AddNewElt(".", "LENGTH", pack.Package.length, 4, 2);
                xml.AddNewElt(".", "HEIGHT", pack.Package.height, 4, 2);
                xml.AddNewElt(".", "WIDTH", pack.Package.width, 4, 2);
                xml.AddNewElt(".", "WEIGHT", pack.Package.weight, 6, 3);
                if (!o.inEU)
                {
                    xml.AddNewElt(".", "ARTICLE");
                    xml.MoveToLastChild();
                    xml.AddNewElt(".", "ITEMS", pack.HowMany.ToString(), 4);
                    xml.AddNewElt(".", "DESCRIPTION", ArticleDescription, 60);
                    xml.AddNewElt(".", "WEIGHT", pack.Package.weight / pack.HowMany, 6, 3);
                    xml.AddNewElt(".", "INVOICEVALUE", pa.ItemInvoiceValue, 15, 2);
                    xml.AddNewElt(".", "INVOICEDESC", ArticleDescription, 60);
                    xml.AddNewElt(".", "HTS", ArticleHts, 15);
                    xml.AddNewElt(".", "COUNTRY", SenderCountry, 3);
                    xml.NavigateThis("..");  //up to PACKAGE
                }
                xml.NavigateThis(".."); //up to DETAILS
            }
        }

        /// <summary>
        /// Add Address fields to the current element. The element should be either SENDER or RECEIVER.
        /// Empty string is allowed for: StreetAddress2 and 3, Province, Account, Vat, ContactDialCode. 
        /// The ACCOUNT element will be omitted when empty.
        /// </summary>
        private void AddAddress(string CompanyName, string StreetAddress1, string StreetAddress2, string StreetAddress3, 
            string City, string Province, string PostCode, string Country, string Account, string Vat, 
            string ContactName, string ContactDialCode, string ContactTelephone, string ContactEmail)
        {
            xml.AddNewElt(".", "COMPANYNAME", CompanyName, 50);
            xml.AddNewElt(".", "STREETADDRESS1", StreetAddress1, 30);
            xml.AddNewElt(".", "STREETADDRESS2", StreetAddress2, 30);
            xml.AddNewElt(".", "STREETADDRESS3", StreetAddress3, 30);
            xml.AddNewElt(".", "CITY", City, 30);
            xml.AddNewElt(".", "PROVINCE", Province, 30);
            xml.AddNewElt(".", "POSTCODE", PostCode, 9);
            xml.AddNewElt(".", "COUNTRY", Country, 3);
            if (Account != null)
                xml.AddNewElt(".", "ACCOUNT", Account, 13);
            xml.AddNewElt(".", "VAT", Vat, 20);
            xml.AddNewElt(".", "CONTACTNAME", ContactName, 50);
            xml.AddNewElt(".", "CONTACTDIALCODE", ContactDialCode, 7);
            xml.AddNewElt(".", "CONTACTTELEPHONE", ContactTelephone, 20);
            xml.AddNewElt(".", "CONTACTEMAIL", ContactEmail, 105);
        }

        /// <summary>
        /// Format a date according to the format string constant in the header of this class
        /// </summary>
        private string TNTDate(DateTime date)
        {
            return date.ToString(DateFormat, dateformatinfo);
        }

        /// <summary>
        /// Validate this TNTShipRequest
        /// </summary>
        /// <param name="schemaUri"></param>
        /// <returns></returns>
        public string Validate(string schemaUri)
        {
            return this.xml.Validate(schemaUri);
        }

        /// <summary>
        /// Write the xml document to a stream, in UTF8 encoding.
        /// </summary>
        public Stream ToUTF8Stream(Stream stream, bool indent=false)
        {
            return this.xml.ToStream(stream, new UTF8Encoding(false, true), indent);
        }
    }
}
