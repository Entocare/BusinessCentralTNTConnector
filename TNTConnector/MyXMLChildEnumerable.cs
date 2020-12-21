using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// Selective child enumerator: Enumerates the children of a given node X that have nodename Y
    /// It returns references to the same document, each time with a different node selected in the navigator for further action
    /// </summary>
    public class MyXMLChildEnumerable : IEnumerable<MyXMLDocument>
    {
        private MyXMLDocument doc;  
        private string multiChildName; 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">MyXMLDocument that has a navigator focussed on the node whose children we want to enumerate</param>
        /// <param name="multiChildName">Name of a node that may occur multiple times under the given node</param>
        public MyXMLChildEnumerable(MyXMLDocument doc, string multiChildName)
        {
            this.doc = doc.PartialClone();  //prevent side effects, we want an independent navigator
            this.multiChildName = multiChildName;
        }

        public IEnumerator<MyXMLDocument> GetEnumerator()
        {
            XPathNavigator nav = doc.nav;
            bool succesfulmove = nav.MoveToFirstChild();
            while (succesfulmove)
            {
                if (nav.LocalName == multiChildName)
                {
                    //return independent copies of the nav, all bundled with a ref to the same underlying document
                    yield return doc.PartialClone();
                }
                succesfulmove = nav.MoveToNext();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
