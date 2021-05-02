using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessCentralTNTConnector;
using BusinessCentralTNTConnector.TNTConnector;
using BCConnector;
using Entocare.NAV;

namespace BusinessCentralTNTConnector
{
    /// <summary>
    /// Form for signing up a single SalesOrder for pickup by TNT
    /// It has a "Sign Up" button for starting the process, as well as various fields for checking progress.
    /// As a result, a maximum of 4 documents are sent to the printer: it tells which docs. 
    /// </summary>
    public partial class ShippingForm : Form
    {
        private bool _busy;                           //requests to tnt in progress
        private ShippingPostalAddress _thisShipment;  //the shipment to sign up with tnt
        private TNTConnectorShipRequest _tntCon;
        private BusinessCentralConnector _ECon;

        public ShippingForm(TNTConnectorShipRequest tntCon, BusinessCentralConnector ECon)
        {
            InitializeComponent();
            _busy = false;
            //_thisShipment = thisShipment;
            _tntCon = tntCon;
            _ECon = ECon;
            _register_tntCon_listeners();
        }

        public void showMyDialog(ShippingPostalAddress thisShipment, Form caller)
        {
            this._thisShipment = thisShipment;
            nameTextBox.Text = _thisShipment.ShipToName;
            orderNumberTextBox.Text = _thisShipment.Number;
            actionTextBox.Text =
                _thisShipment.PackageTrackingNo == "" ?
                    "Sign up a new package at TNT and print the documents" :
                    "Print documents only (package already signed up)";
            this.ShowDialog(caller);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
            this.clear();
        }

        private void clear()
        {
            //reset instance variables
            this._thisShipment = null;
            //reset inputs
            this.nameTextBox.ResetText();
            this.orderNumberTextBox.ResetText();
            this.actionTextBox.ResetText();
            this.messageTextBox.ResetText();
            this.labelTextBox.ResetText();
            this.manifestTextBox.ResetText();
            this.connoteTextBox.ResetText();
            this.invoiceTextBox.ResetText();
            this.trackingnoTextBox.ResetText();
            this.trackingnoMessageTextBox.ResetText();
            this.signUpButton.Enabled = true;
        }

        private void ShippingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_busy || _tntCon.printQueue.Status.Count >0)
            {
                DialogResult result = MessageBox.Show(
                    "Requests to TNT have not finished, or are still printing. Are you sure you want to close this window?",
                    "Warning",
                    MessageBoxButtons.OKCancel
                    );
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;  //cancel the closing event
                }
            }
        }

        private void signUpButton_Click(object sender, EventArgs e)
        {
            _busy = true;
            messageTextBox.Text = "Sending Consignment Request...";
            signUpButton.Enabled = false;
            TNTShipRequest.RequestTypes type = (_thisShipment.PackageTrackingNo == "") ?
                TNTShipRequest.RequestTypes.Full :
                TNTShipRequest.RequestTypes.PrintOnly;
            try
            {
                _tntCon.NewShipRequest(_thisShipment.ShipmentDate.Value, type);
                _tntCon.AddConsignment(new SalesOrder()
                {
                    SalesOrderId = _thisShipment.Number,      //It cannot be the GUID, it's too long; this ones unique too
                    SalesOrderNumber = _thisShipment.Number,  //This could be a number assigned by the customer (not available now)
                    InvoiceValue = (double)_thisShipment.AmountIncludingVAT,
                    Vat = _thisShipment.VatRegistrationNo,
                    //address
                    CompanyName = _thisShipment.ShipToName + " " + _thisShipment.ShipToName2,
                    StreetAddress1 = _thisShipment.ShipToAddress,
                    StreetAddress2 = _thisShipment.ShipToAddress2,
                    StreetAddress3 = "",
                    City = _thisShipment.ShipToCity,
                    Province = "",
                    PostCode = _thisShipment.ShipToPostCode,
                    CountryAbb = _thisShipment.ShipToCountryRegionCode,
                    //contact
                    ContactName = _thisShipment.ShipToContactName, //probleem: ship-to Contact niet correct gejoined vanaf Sales Header aan AL-kant (op naam, niet op id). Dit kan dubbele rijen geven.
                    ContactDialCode = "000",
                    ContactTelephone = _thisShipment.ShipToContactPhone,
                    ContactEmail = _thisShipment.ShipToContactEmail,
                    //shipping agent specific
                    ShippingAgentID = _thisShipment.ShippingAgentCode,  //TNT does not need it. Add a filter somewhere.
                    ShippingAgentService = correctTNTServiceCode(
                        _thisShipment.ShippingAgentServiceCode, _thisShipment.ShipToCountryRegionCode),
                    DeliveryInstructions = (bool)_thisShipment.ShipOptionHoldForPickup ? "Hold for pickup" : "",
                    //package info
                    PackageFormatCode = _thisShipment.ShippingPackageDimensions,
                    PackageTrackingNo = _thisShipment.PackageTrackingNo,
                });
                _tntCon.StartSendingAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating request for TNT: " + ex.Message);
                _busy = false;
            }
        }

        /// <summary>
        /// Create a correct TNT service code from service codes registered for TNT in Business Central
        /// - Omit the "TNT-" prefix
        /// - Add the letter "N" (for Non-doc) in case the country is not NL
        /// </summary>
        /// <param name="ServiceCode">Service code registered for TNT in Business Central</param>
        /// <param name="CountryCode">2-letter Country code</param>
        /// <returns>the corrected code</returns>
        private string correctTNTServiceCode(string ServiceCode, string CountryCode)
        {
            if (ServiceCode.StartsWith("TNT-"))
            {
                ServiceCode = ServiceCode.Substring(4);
            }
            if (CountryCode != "NL")
            {
                ServiceCode = ServiceCode + "N";
            }
            return ServiceCode;
        }

        //listeners for events from the TNTConnector!
        private void _tntCon_AccessCodeReceived(object sender, EventArgs e)
        {
            messageTextBox.Text = "Fetching result...";
        }

        private async void _tntCon_ConsignmentSuccess(object sender, TNTConnectorShipRequest.ConsignmentSuccessArgs e)
        {
            string trackNo = e.ConNumber;
            trackingnoTextBox.Text = trackNo;
            _thisShipment.PackageTrackingNo = trackNo; //update the locally cached orders, visible on te orders screen, to accomodate reprint

            //Send it back to Business Central - a follow-up async process. We cannot return a Task here so we handle all errors.
            try
            {
                //1.Get the original row with recent Etag
                OrderPackageTrackingNo track = await _ECon.Ectx.OrderPackageTrackingNos.ByKey(_thisShipment.Id.Value).GetValueAsync();
                var wrapper = _ECon.CreateChangeListener<OrderPackageTrackingNo>(track);
                //2.Set the new trackingNo
                track.PackageTrackingNo = trackNo;
                var resp = await wrapper.SaveChangesAsync();
                //if we get here it should be OK
                trackingnoMessageTextBox.Text = "Saved in Business Central";
            }
            catch (Exception ex)
            {
                trackingnoMessageTextBox.Text = "Not saved - error";
                MessageBox.Show("TNT PackageTrackingNo " + trackNo + " was not correctly received by Business Central. Error: " + ex.Message);
            }
        }

        private void _tntCon_ResultReceived(object sender, TNTConnectorShipRequest.ResultReceivedArgs e)
        {
            if (e.LabelStatus == TNTConnectorShipRequest.DocStatusses.OnTNTServer)
                labelTextBox.Text = "On TNT Server";
            if (e.ManifestStatus == TNTConnectorShipRequest.DocStatusses.OnTNTServer)
                manifestTextBox.Text = "On TNT Server";
            if (e.ConnoteStatus == TNTConnectorShipRequest.DocStatusses.OnTNTServer)
                connoteTextBox.Text = "On TNT Server";
            if (e.InvoiceStatus == TNTConnectorShipRequest.DocStatusses.OnTNTServer)
                invoiceTextBox.Text = "On TNT Server";
        }

        private void _tntCon_DocumentRequest(object sender, TNTConnectorShipRequest.DocEventArgs e)
        {
            switch(e.DocType)
            {
                case TNTConnectorShipRequest.DocTypes.Label:
                    labelTextBox.Text = "Fetching...";
                    break;
                case TNTConnectorShipRequest.DocTypes.Manifest:
                    manifestTextBox.Text = "Fetching...";
                    break;
                case TNTConnectorShipRequest.DocTypes.Connote:
                    connoteTextBox.Text = "Fetching...";
                    break;
                case TNTConnectorShipRequest.DocTypes.Invoice:
                    invoiceTextBox.Text = "Fetching...";
                    break;
            }
        }

        private void _tntCon_DocumentReceived(object sender, TNTConnectorShipRequest.DocEventArgs e)
        {
            switch (e.DocType)
            {
                case TNTConnectorShipRequest.DocTypes.Label:
                    labelTextBox.Text = "Sent to Printer.";
                    break;
                case TNTConnectorShipRequest.DocTypes.Manifest:
                    manifestTextBox.Text = "Sent to Printer.";
                    break;
                case TNTConnectorShipRequest.DocTypes.Connote:
                    connoteTextBox.Text = "Sent to Printer.";
                    break;
                case TNTConnectorShipRequest.DocTypes.Invoice:
                    invoiceTextBox.Text = "Sent to Printer.";
                    break;
            }
        }

        private void _tntCon_ShipRequestReady(object sender, EventArgs e)
        {
            _busy = false;
            int stillPrinting = _tntCon.printQueue.Status.Count;
            messageTextBox.Text = "TNT communication ready...!" + (stillPrinting > 0 ? " " + stillPrinting + " Documents still printing." : "");
        }

        //Error events

        private void _tntCon_ErrorOccurred(object sender, TNTConnectorShipRequest.ErrorArgs e)
        {
            _busy = false; //not sure, but nicer to close without further warnings
            messageTextBox.Text = "Error: " + e.Description + " (" + e.ErrorNumber + ")";
        }

        private void _tntCon_ConsignmentNoSuccess(object sender, TNTConnectorShipRequest.ConsignmentNoSuccessArgs e)
        {
            _busy = false; //sure
            messageTextBox.Text = "Error: order number " + e.ConRef + " refused by TNT server.";
        }

        private void _register_tntCon_listeners()
        {
            _tntCon.AccessCodeReceived += _tntCon_AccessCodeReceived;
            _tntCon.ConsignmentSuccess += _tntCon_ConsignmentSuccess;
            _tntCon.ResultReceived += _tntCon_ResultReceived;
            _tntCon.DocumentRequest += _tntCon_DocumentRequest;
            _tntCon.DocumentReceived += _tntCon_DocumentReceived;
            _tntCon.ShipRequestReady += _tntCon_ShipRequestReady;

            _tntCon.PostError += _tntCon_ErrorOccurred;
            _tntCon.ShipRequestError += _tntCon_ErrorOccurred;
            _tntCon.ShipResultError += _tntCon_ErrorOccurred;
            _tntCon.DocumentFailedToLoad += _tntCon_ErrorOccurred;
            _tntCon.ConsignmentNoSuccess += _tntCon_ConsignmentNoSuccess;
        }
    }
}
