using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessCentralTNTConnector.TNTConnector;

namespace BusinessCentralTNTConnector
{
    static class Program
    {
        private const bool Debug = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Net.ServicePointManager.DnsRefreshTimeout = 120000; //=default, -1 is infinite; how long to cache a dns resolution
            //Any Console testing:
            //TNTResultTest.DoTheTests();
            //TNTShipRequestTest.DoTheTests();
            //TNTConnectorShipRequestTest.DoTheTests();

            //Forms part startup
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MDIParent1 mainform = new MDIParent1(Debug);
            Application.Run(mainform);
        }
    }
}
