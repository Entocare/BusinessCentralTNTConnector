using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Entocare.NAV;

namespace BusinessCentralTNTConnector
{
    /// <summary>
    /// Form showing orders from BC, from which to choose for sending
    /// </summary>
    public partial class OrdersForm : Form
    {
        public static readonly string BCShippingAgentTNTCode = "TNT";

        private MDIParent1 _owner;  //the MDIParent is not the MDIParent1 object but its superclass

        private ShippingForm shipping;

        public OrdersForm(MDIParent1 owner)
        {
            _owner = owner;
            InitializeComponent();
            shippingPostalAddressBindingSource.AllowNew = false; //locked
            dateTimePicker1.Value = DateTime.Today;
            shipping = new ShippingForm(_owner.TNTCon, _owner.ECon);
            GetOrders();   //async probleempje, waar moeten de excepties naartoe?
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            GetOrders();
        }

        private async Task GetOrders()
        {
            try
            {
                var orders = await _owner.ECon.Ectx.ShippingPostalAddresses
                    .AddQueryOption("$filter", "shipmentDate eq " + dateTimePicker1.Value.ToString("yyyy-MM-dd"))
                    .GetAllPagesAsync();
                //The BindingSource is set and configured in the Designer! (see other part of partial class)
                shippingPostalAddressBindingSource.Clear();
                shippingPostalAddressBindingSource.AllowNew = true;
                foreach (ShippingPostalAddress order in orders)
                {
                    shippingPostalAddressBindingSource.Add(order);
                }
                shippingPostalAddressBindingSource.AllowNew = false;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null)
                {
                    msg += ": " + e.InnerException.Message;
                    if (e.InnerException.InnerException != null)
                    {
                        msg += ": " + e.InnerException.InnerException.Message;
                    }
                }
                MessageBox.Show(
                    "Error getting Sales orders from Business Central:\n" + 
                    "- is the internet connection working?\n" +
                    "- is Wolters app installed in Business Central?\n\n" + 
                    "Error message: " + msg);
            }
        }

        private void signUpButton_Click(object sender, EventArgs e)
        {
            ShippingPostalAddress cur = (ShippingPostalAddress)shippingPostalAddressBindingSource.Current;
            if (
                cur != null && 
                cur.SumStatus == ShippingPostalAddress.SumStatusses.ReadyForShip &&
                cur.ShippingAgentCode == BCShippingAgentTNTCode
            )
            {
                shipping.showMyDialog(cur, this);
            }
            else
            {
                if (cur == null)
                    MessageBox.Show("Please select an order first");
                else if (cur.SumStatus != ShippingPostalAddress.SumStatusses.ReadyForShip)
                    MessageBox.Show("Not ready for shipping!");
                else if (cur.ShippingAgentCode != BCShippingAgentTNTCode)
                    MessageBox.Show("Not a TNT shipment!");
                else
                    MessageBox.Show("Unknown problem...");
            }
        }
    }
}
