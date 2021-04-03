using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    public static class TNTShipRequestTest
    {
        public const string ShipRequestSchemaUri = @".\TNTConnector\xsd\ShipmentRequestIN.xsd";

        public const string OutputFile = @"D:\ShipRequest.xml";  //@".\TNTConnector\ShipRequest.xml"

        public static void DoTheTests()
        {
            TNTPackageAnalyser pa = new TNTPackageAnalyser();
            TNTShipRequest req = new TNTShipRequest(new DateTime(2019, 1, 14), TNTShipRequest.RequestTypes.Full, pa, Debug: true);

            //add one or more consignments to the request
            SalesOrder o1 = new SalesOrder()
            {
                SalesOrderId = "a1234r",
                SalesOrderNumber = "1",
                InvoiceValue = 99.99,
                Vat = "7668880",  //other customer data
                //address
                CompanyName = "Andermatt BIOCONTROL AG",
                StreetAddress1 = "Otztal 67",
                StreetAddress2 = "Who has a streataddress 2?",
                StreetAddress3 = "",
                City = "Grossdietwil",
                Province = "",
                PostCode = "6146",
                CountryAbb = "CH",
                //contact
                ContactName = "Kathrin Flückiger",
                ContactDialCode = "000",
                ContactTelephone = "41629175005",
                ContactEmail = "w.h.kaper@uva.nl",
                //shipping agent specific
                ShippingAgentID = "1",
                ShippingAgentService = "15N",   //N for abroad!
                DeliveryInstructions = "Hold for pickup",
                //package info
                PackageFormatCode = "S+M+M",
            };
            req.AddConsignment(o1); 

            SalesOrder o2 = o1.Clone();
            o2.SalesOrderId = "b1234p";
            o2.SalesOrderNumber = "2";
            o2.CompanyName = "Agentschap Plantentuin Meise";
            o2.PackageFormatCode = "XL";
            o2.ShippingAgentService = "15N";
            o2.City = "Meise";
            o2.PostCode = "1860";
            o2.CountryAbb = "BE";
            req.AddConsignment(o2);

            //check and print the request to a file
            string errors = req.Validate(ShipRequestSchemaUri);
            if (errors.Length > 0)
            {
                throw new InvalidOperationException("Schema validation error(s): \r\n" + errors);
            }

            using (FileStream fs = new FileStream(OutputFile, FileMode.OpenOrCreate))
            {
                req.ToUTF8Stream(fs, indent: true);
            }
        }
    }
}
