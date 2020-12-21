using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// Wrapper for an XMLDocument that gives various extras.
    /// The XMLDocument itself is also exposed, to avoid wrapping all of its methods.
    /// 
    /// It contains a navigator that enables navigating the document and focussing on one specific node at at time.
    /// 
    /// - Navigate to a location given by an XPath, while not loosing anything of the context.
    /// - Get an iterator over the (multiple) children of a given node of a given type.
    /// - Get an index for the (multiple) children of a given node of a given type, indexed on a chosen property
    ///   that's accessible by a given "SelectSingleNode" kind of XPath, from each of the children.
    /// </summary>
    public class MyXMLDocument
    {
        public XmlDocument docRW { get; private set; }
        public XPathDocument docR { get; private set; }
        public XPathNavigator nav { get; private set; }

        public bool IsWritableDocWrapped { get { return docRW != null; } }
        public bool IsReadonlyDocWrapped { get { return docR != null; } }

        public readonly NumberFormatInfo nf;
        private StringWriter validationerrors;

        /// <summary>
        /// Private constructor, enables partial cloning of this document
        /// </summary>
        private MyXMLDocument(XPathNavigator nav, XmlDocument docRW = null, XPathDocument docR = null, NumberFormatInfo nf = null)
        {
            this.nav = nav;
            this.docRW = docRW;
            this.docR = docR;
            this.nf = GetNumberFormatInfo(nf);
            this.validationerrors = new StringWriter();
        }

        /// <summary>
        /// Constructor, creates a navigable representation of a document from an XmlReader
        /// </summary>
        /// <param name="reader">the XmlReader as source of the document</param>
        /// <param name="readOnly">create as readonly? read/write otherwise</param>
        public MyXMLDocument(XmlReader reader, bool readOnly = true, NumberFormatInfo nf = null)
        {
            if (readOnly)
            {
                this.docR = new XPathDocument(reader);
                this.nav = docR.CreateNavigator();
            }
            else
            {
                this.docRW = new XmlDocument();
                this.docRW.Load(reader);
                this.nav = docRW.CreateNavigator();
            }
            this.nf = GetNumberFormatInfo(nf);
        }

        /// <summary>
        /// Start from empty document - only sensible if we can write to it
        /// </summary>
        public MyXMLDocument(NumberFormatInfo nf = null)
        {
            this.docRW = new XmlDocument();
            this.nav = docRW.CreateNavigator();
            this.nf = GetNumberFormatInfo(nf);
        }

        /// <summary>
        /// Get a partial clone of the document
        /// The clone will have an independent "current" navigation position, but the underlying document is just 1 thing
        /// </summary>
        /// <returns></returns>
        public MyXMLDocument PartialClone()
        {
            return new MyXMLDocument(this.nav.Clone(), docRW, docR, nf);
        }

        /// <summary>
        /// Navigate to a new position, relative to the navigation position of 'this' document.
        /// Returns new MyXMLDocument, it has no effects on the original one.
        /// </summary>
        /// <param name="xpath">relative XPath expression identifying a single node: the new navigation position</param>
        /// <returns>A partial clone of this doc, focussed on the new navigation position</returns>
        public MyXMLDocument Navigate(XPathExpression xpath)
        {
            MyXMLDocument doc2 = PartialClone();
            doc2.nav = nav.SelectSingleNode(xpath);
            return doc2;
        }

        /// <summary>
        /// Navigate to a new position, relative to the navigation position of 'this' document.
        /// It changes the navigator of this document instance. 
        /// Use it in tandem with the (docRW) editing methods, to leverage the use of relative xpaths.
        /// </summary>
        /// <param name="xpath">relative XPath expression identifying a single node: the new navigation position</param>
        public void NavigateThis(string xpath)
        {
            this.nav = nav.SelectSingleNode(xpath);
        }

        /// <summary>
        /// Navigate to the last child of the current node
        /// The last child does not need to be an element, it can be an attribute or anything.
        /// </summary>
        public void MoveToLastChild()
        {
            XmlNode curElt = (XmlNode)this.nav.UnderlyingObject;
            XmlNode lastCh = curElt.LastChild;
            this.nav = lastCh.CreateNavigator();
        }

        /// <summary>
        /// Get an IEnumerable of the children of a given node X that have nodeName Y
        /// The IENumerable returns MyXMLDocument instances, each focussed on another node.
        /// </summary>
        /// <param name="xpath">relative XPath expression identifying a single node: the node X whose children we want to enumerate</param>
        /// <param name="multiChildName">name Y of an element that may occur multiple times as child of X</param>
        /// <returns>the Enumerable</returns>
        public MyXMLChildEnumerable GetChildEnumerable(XPathExpression xpath, string multiChildName)
        {
            return new MyXMLChildEnumerable(this.Navigate(xpath), multiChildName);
        }

        /// <summary>
        /// Index the children of a given node X that have common nodeName Y on a property Z
        /// </summary>
        /// <param name="xpath">relative XPath identifying the node X whose children we want to index</param>
        /// <param name="multiChildName">name Y of an element that may occur multiple times as child of X</param>
        /// <param name="propertyXPath">XPath relative to each Y-child, identifying the property to use as a key</param>
        /// <returns></returns>
        public MyXMLChildIndex GetChildIndex(XPathExpression xpath, string multiChildName, XPathExpression propertyXPath)
        {
            return new MyXMLChildIndex(this.Navigate(xpath), multiChildName, propertyXPath);
        }

        //convenience methods

        /// <summary>
        /// Check if a given xpathexpression selects at least one node in the document.
        /// If the xpath is relative, it will be evaluated with respect to the current node as known by the wrapped navigator.
        /// </summary>
        /// <param name="xpath">the xpath to check</param>
        public bool NodeExists(XPathExpression xpath)
        {
            return (this.nav.SelectSingleNode(xpath) != null);
        }

        /// <summary>
        /// Check if a given xpathexpression selects at least one node in the document.
        /// If the xpath is relative, it will be evaluated with respect to the current node as known by the wrapped navigator.
        /// </summary>
        /// <param name="xpath">the xpath to check</param>
        public bool NodeExists(string xpath)
        {
            return (this.nav.SelectSingleNode(xpath) != null);
        }

        /// <summary>
        /// Get a primitive value out of the document, as a string.
        /// It throws exceptions when the node does not exist, or is not of a kind that has a primitive value.
        /// </summary>
        /// <param name="xpath">an xpath that points to a primitive value</param>
        public string GetStringValue(XPathExpression xpath)
        {
            var nav2 = this.nav.SelectSingleNode(xpath);
            return this.GetTheValue(nav2, xpath.Expression);
        }

        /// <summary>
        /// Get a primitive value out of the document, as a string.
        /// It throws exceptions when the node does not exist, or is not of a kind that has a primitive value.
        /// </summary>
        /// <param name="xpath">an xpath that points to a primitive value</param>
        public string GetStringValue(string xpath)
        {
            var nav2 = this.nav.SelectSingleNode(xpath);
            return this.GetTheValue(nav2, xpath);
        }

        private string GetTheValue(XPathNavigator nav2, string xpath)
        {
            if (nav2 != null)
            {
                if (nav2.NodeType == XPathNodeType.Text || nav2.NodeType == XPathNodeType.Attribute)
                {
                    return nav2.Value;
                }
                else throw new InvalidOperationException("The XPath '" + xpath + "' does not point to a node that has a primitive value.");
            }
            else throw new InvalidOperationException("The XPath '" + xpath + "' does not select a single node.");
        }

        //Convenience methods meant for writing to DocRW
        //The xpaths below all should return a single node! They are evaluated by the nav, so, can be relative.

        /// <summary>
        /// Add a new child element at a given xpath, and give it a floating point or double numeric value
        /// that has to be formatted using a given precision, and the initialised culturespecific numberformat
        /// </summary>
        /// <param name="xpath">xpath of single element where child element is to be added</param>
        /// <param name="name">name of the new child element</param>
        /// <param name="numberValue">float or double value that has to be formatted</param>
        /// <param name="maxLength">maximum length of the formatted number as a string</param>
        /// <param name="precision">Number of fractional digits to show</param>
        public void AddNewElt(string xpath, string name, double numberValue, int maxLength, int precision)
        {
            string textvalue = numberValue.ToString("F"+ precision, nf);
            AddNewElt(xpath, name, textvalue, maxLength);
        }

        private NumberFormatInfo GetNumberFormatInfo(NumberFormatInfo nf = null)
        {
            return (nf != null) ? nf : new NumberFormatInfo(); //international culture, e.g. decimal separator is "."
        }

        /// <summary>
        /// Add a new child element at a given xpath
        /// </summary>
        /// <param name="xpath">xpath of single element where child element is to be added</param>
        /// <param name="name">name of the new child element</param>
        /// <param name="textvalue">(optional) text value for the new element</param>
        /// <param name="maxLength">(optional)  maximum length of the value, or null for no check</param>
        public void AddNewElt(string xpath, string name, string textvalue = null, int? maxLength = null)
        {
            CheckLength(xpath, name, textvalue, maxLength);
            nav.SelectSingleNode(xpath).AppendChildElement(null, name, null, textvalue);
        }

        /// <summary>
        /// Add a new attribute at a given xpath
        /// </summary>
        /// <param name="xpath">xpath of single element where attribute is to be added</param>
        /// <param name="attName">name of the new attribute</param>
        /// <param name="attValue">(optional) string value of the new attribute</param>
        /// <param name="maxLength">(optional) maximum length of the value, or null for no check</param>
        public void AddNewAttr(string xpath, string attName, string attValue = null, int? maxLength = null)
        {
            CheckLength(xpath, attName, attValue, maxLength);
            nav.SelectSingleNode(xpath).CreateAttribute(null, attName, null, attValue);
        }

        /// <summary>
        /// Add an XmlElement as a child at a given xpath
        /// </summary>
        /// <param name="xpath">xpath of single element where child element is to be added</param>
        /// <param name="elt">the element to be added</param>
        public void AddElt(string xpath, XmlElement elt)
        {
            XPathNavigator elt2 = elt.CreateNavigator();
            nav.SelectSingleNode(xpath).AppendChild(elt2);
        }

        /// <summary>
        /// Check the length of a string, which is about to be used as textvalue in an element or attribute
        /// </summary>
        /// <param name="xpath">xpath where value is to be used, for use in error message</param>
        /// <param name="name">name of new element or attribute where value is to be used, for use  in error message</param>
        /// <param name="value">the value to be checked</param>
        /// <param name="maxLength">max length allowed for the value</param>
        private void CheckLength(string xpath, string name, string value, int? maxLength)
        {
            if (maxLength != null && value.Length > maxLength)
            {
                throw new InvalidOperationException("Maximum length " + maxLength + " exceeded for value of new child " + name + " at xpath " + xpath + ".");
            }
        }

        /// <summary>
        /// Output the (writable) document to a stream,in a chosen encoding
        /// </summary>
        /// <param name="stream">the stream to write the document to</param>
        /// <param name="enc">the encoding to use</param>
        /// <param name="indent">indent the xml for readability?</param>
        /// <returns>the same steam as the input stream</returns>
        public Stream ToStream(Stream stream, Encoding enc, bool indent = false)
        {
            if (docRW != null)
            {
                XmlWriterSettings set = new XmlWriterSettings()
                {
                    Encoding = enc,
                    Indent = indent,
                };
                XmlWriter wri2 = XmlWriter.Create(stream, set);
                this.docRW.Save(wri2);
                return stream;
            }
            else
            {
                throw new NotSupportedException("ToStream is not supported for a readonly xml documents.");
            }
        }

        /// <summary>
        /// Validate this (writable) xml document according to a schema, and return all errors in a single string
        /// </summary>
        /// <param name="schemaUri">URI of the schema to use</param>
        /// <returns>Errors logged by the validator, one error per line.</returns>
        public string Validate(string schemaUri)
        {
            if (docRW != null)
            {
                validationerrors = new StringWriter(); //clean start
                docRW.Schemas.Add(null, schemaUri);
                docRW.Validate(ValidationErrorHandler);  //see next method, it's the handler
                return validationerrors.ToString();
            }
            else
            {
                throw new NotSupportedException("Validation is not supported for a readonly xml documents.");
            }
        }

        private void ValidationErrorHandler(object sender, ValidationEventArgs e)
        {
            validationerrors.WriteLine(e.Severity.ToString() + ": " + e.Message);
        }


    }
}
