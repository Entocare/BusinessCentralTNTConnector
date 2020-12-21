using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    public static class TNTConnectorShipRequestTest
    {
        public static async Task DoTheTests()
        {
            TNTConnectorShipRequest con = new TNTConnectorShipRequest(Debug: true);

            DateTime collectionDate = new DateTime(2019, 02, 07);
            con.NewShipRequest(collectionDate, TNTShipRequest.RequestTypes.Full);

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
            con.AddConsignment(o1);

            SalesOrder o2 = o1.Clone();  //Clone is OK... Goodsvalue is gekopieerd.
            o2.SalesOrderId = "b1234p";
            o2.SalesOrderNumber = "2";
            o2.CompanyName = "Agentschap Plantentuin Meise";
            o2.PackageFormatCode = "XL";
            o2.ShippingAgentService = "15N";
            o2.City = "Meise";
            o2.PostCode = "1860";
            o2.CountryAbb = "BE";
            con.AddConsignment(o2);

            await con.StartSendingAsync();
            //After this the process should run mainly by itself.
            //Put breakpoints after each await if you want to see something happening
            //End result should be some docs written to the configured ourput dir

            //Breakpoint here, otherwise spoil the previous test
            //Now we're gonna act like we want to re-receive the previously received docs
            con = new TNTConnectorShipRequest(Debug: true);
            con.NewShipRequest(collectionDate, TNTShipRequest.RequestTypes.PrintOnly);
            con.AddConsignment(o1);
            con.AddConsignment(o2);
            await con.StartSendingAsync();
            //same remarks here

            //TODO: test the events at a point when they mean something: in the form that's going to show the messages.
        }
    }
}
