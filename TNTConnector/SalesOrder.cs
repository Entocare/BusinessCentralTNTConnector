using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// Represents what TNT needs to know about the SalesOrder to send
    /// </summary>
    public class SalesOrder
    {
        public string SalesOrderId { get; set; }
        public string SalesOrderNumber { get; set; }
        public double InvoiceValue { get; set; }  //AL can deliver it on header level for sure, or we do some adding client-side

        //Receiver Address
        public string CompanyName { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string StreetAddress3 { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostCode { get; set; }
        public string CountryAbb { get; set; }
        public bool inEU { get { return EUCountries.inEU(CountryAbb); } }

        //Receiver Contact
        public string ContactName { get; set; }
        public string ContactDialCode { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }

        //Other Receiver / Customer Info
        public string Vat { get; set; }

        //Carrier-specific Order Info
        public string ShippingAgentID { get; set; }
        /// <summary>
        /// 2-digit TNT Service code (like '09' for "9:00 uur service" or '48' for "within 48 hours"), 
        /// with a letter "N" appended ONLY if the destination country is NOT NL
        /// </summary>
        public string ShippingAgentService { get; set; }
        public string PackageTrackingNo { get; set; }
        public string DeliveryInstructions { get; set; }

        /// <summary>
        /// String of elementary Package format codes (like: S, M, L, XL) defined by Entocare, + as separator
        /// </summary>
        public string PackageFormatCode { get; set; }

        public SalesOrder Clone()
        {
            return new SalesOrder()
            {
                SalesOrderId = this.SalesOrderId,
                SalesOrderNumber = this.SalesOrderNumber,
                InvoiceValue = this.InvoiceValue,
                CompanyName = this.CompanyName,
                StreetAddress1 = this.StreetAddress1,
                StreetAddress2 = this.StreetAddress2,
                StreetAddress3 = this.StreetAddress3,
                City = this.City,
                Province = this.Province,
                PostCode = this.PostCode,
                CountryAbb = this.CountryAbb,
                ContactName = this.ContactName,
                ContactDialCode = this.ContactDialCode,
                ContactTelephone = this.ContactTelephone,
                ContactEmail = this.ContactEmail,
                Vat = this.Vat,
                ShippingAgentID = this.ShippingAgentID,
                ShippingAgentService = this.ShippingAgentService,
                PackageTrackingNo = this.PackageTrackingNo,
                DeliveryInstructions = this.DeliveryInstructions,
                PackageFormatCode = this.PackageFormatCode,
            };
        }
    }
}
