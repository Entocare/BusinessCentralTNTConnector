using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    public static class TNTResultTest
    {
        public const string FilePath = @"C:\Admini\TNT_ExpressConnect_examples\Results\";

        /// <summary>
        /// Test reading a TNT Result document, using filed examples
        /// </summary>
        public static void DoTheTests()
        {
            //examples to choose from
            string[] filenames = new string[] {
                "multi_3con_createship.txt",
                "typical_1con_createship.txt",
                "typical_1con_createship_just2docs.txt",
                "printonly.txt",
                "error.txt",
                "runtime_error.txt",
            };

            //edit this line to choose an example
            string fullpath = FilePath + filenames[2];

            //edit this line to choose one of the two indexing options
            TNTResult.IndexBy indexBy = TNTResult.IndexBy.CONREF;

            //initialize the class to test
            XmlReader XReader = XmlReader.Create(fullpath);
            TNTResult res = new TNTResult(XReader, indexBy);

            //look for error messages
            if (res.hasAnyError)
            {
                Debug.Print("Error: " + res.GetFirstErrorDescr());
            }

            if (res.OK)
            {
                //worthy of further investigation

                //available documents on TNT server
                Debug.Print("IsConnoteCreated: " + res.IsConnoteCreated);
                Debug.Print("IsInvoiceCreated: " + res.IsInvoiceCreated);
                Debug.Print("IsLabelCreated: " + res.IsLabelCreated);
                Debug.Print("IsManifestCreated: " + res.IsManifestCreated);

                //iterate the consignments in the doc and get their properties
                foreach (var conId in res)
                {
                    Debug.Print("Id value used for indexing: " + conId);
                    Debug.Print("ConRef: " + res.ConRef);
                    Debug.Print("ConNumber: " + res.ConNumber);
                    Debug.Print("Success: " + res.Success);
                }
            }
            else
            {
                Debug.Print("The document is not worthy of further investigation.");
            }
        }
    }
}
