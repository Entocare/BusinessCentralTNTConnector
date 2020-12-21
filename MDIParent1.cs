using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessCentralTNTConnector.TNTConnector;
using BCConnector;


namespace BusinessCentralTNTConnector
{
    /// <summary>
    /// The main window of the app in which the two child windows will appear
    /// </summary>
    public partial class MDIParent1 : Form
    {
        public TNTConnectorShipRequest TNTCon { get; private set; }
        public BusinessCentralConnector ECon { get; private set; }
        private Form _ordersForm;

        public MDIParent1(bool Debug = false)
        {
            InitializeComponent();
            //initialize the 2 needed connectors
            TNTCon = new TNTConnectorShipRequest(Debug);
            ECon = BusinessCentralConnector.GetEntocareBCConnector(Debug);

            //show the orders form
            _ordersForm = new OrdersForm(this);
            _ordersForm.MdiParent = this;
            _ordersForm.Show();
        }

        private void sendOrdersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_ordersForm == null || !_ordersForm.Created)
            {
                _ordersForm = new OrdersForm(this);
                _ordersForm.MdiParent = this;
            }
            _ordersForm.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new AboutBCTNT();
            about.MdiParent = this;
            about.Show();
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MDIParent1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int stillPrinting = TNTCon.printQueue.Status.Count;
            if (stillPrinting >0)
            {
                DialogResult result = MessageBox.Show(
                    stillPrinting + " Documents are still printing. Are you sure you want to close this app.?",
                    "Warning",
                    MessageBoxButtons.OKCancel
                    );
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;  //cancel the closing event
                }
            }
        }
    }
}
