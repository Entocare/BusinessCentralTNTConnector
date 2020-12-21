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
    /// Index the children of a given node X that have the same name Y on a meaningful (string) property Z, identified by an XPath expression.
    /// Allows random access to these children based on values of Z.
    /// </summary>
    public class MyXMLChildIndex
    {
        private Dictionary<string, MyXMLDocument> index;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">MyXMLDocument that has a navigator focussed on the node whose children we want to index</param>
        /// <param name="multiChildName">Name of a node that may occur multiple times under the given node</param>
        public MyXMLChildIndex(MyXMLDocument doc, string multiChildName, XPathExpression propertyXPath)
        {
            this.index = new Dictionary<string, MyXMLDocument>();
            var iter = new MyXMLChildEnumerable(doc, multiChildName);
            foreach (MyXMLDocument node in iter)
            {
                string key = node.nav.SelectSingleNode(propertyXPath).Value;
                index.Add(key, node);
            }
        }

        /// <summary>
        /// Get a child, using the index
        /// </summary>
        public MyXMLDocument GetChildByIndexValue(string indexValue)
        {
            MyXMLDocument doc;
            if (index.TryGetValue(indexValue, out doc) )
            {
                return doc;
            }
            else return null;
        }

        /// <summary>
        /// Get the number of children, may be zero
        /// </summary>
        public int Count { get { return index.Count; } }
    }
}
