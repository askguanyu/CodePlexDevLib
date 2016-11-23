//-----------------------------------------------------------------------
// <copyright file="FluentXml.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Xml
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Provides a class for specifying fluent Xml behavior at run time.
    /// </summary>
    public class FluentXml
    {
        /// <summary>
        /// Field _xmlNode.
        /// </summary>
        private readonly XmlNode _xmlNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentXml"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="fromXmlString">true if the source is Xml string; false if the source is Xml file.</param>
        private FluentXml(string source, bool fromXmlString)
        {
            var xmlDocument = new XmlDocument();

            if (fromXmlString)
            {
                xmlDocument.LoadXml(source);
            }
            else
            {
                xmlDocument.Load(source);
            }

            this._xmlNode = xmlDocument as XmlNode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentXml"/> class.
        /// </summary>
        /// <param name="inStream">The stream containing the XML document to load.</param>
        private FluentXml(Stream inStream)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(inStream);

            this._xmlNode = xmlDocument as XmlNode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentXml"/> class.
        /// </summary>
        /// <param name="txtReader">The TextReader used to feed the XML data.</param>
        private FluentXml(TextReader txtReader)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(txtReader);

            this._xmlNode = xmlDocument as XmlNode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentXml"/> class.
        /// </summary>
        /// <param name="reader">The XmlReader used to feed the XML data.</param>
        private FluentXml(XmlReader reader)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);

            this._xmlNode = xmlDocument as XmlNode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentXml"/> class.
        /// </summary>
        /// <param name="xmlNode">The XML node used to feed the XML data.</param>
        private FluentXml(XmlNode xmlNode)
        {
            this._xmlNode = xmlNode;
        }

        /// <summary>
        /// Gets the local name of the node.
        /// </summary>
        public string Name
        {
            get
            {
                if (this._xmlNode != null)
                {
                    return this._xmlNode.LocalName;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the attributes of the node.
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                if (this._xmlNode != null)
                {
                    foreach (XmlAttribute xmlAttribute in this._xmlNode.Attributes)
                    {
                        result.Add(xmlAttribute.Name, xmlAttribute.Value);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the markup representing this node and all its child nodes.
        /// </summary>
        public string OuterXml
        {
            get
            {
                if (this._xmlNode != null)
                {
                    return this._xmlNode.OuterXml;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the markup representing only the child nodes of this node.
        /// </summary>
        public string InnerXml
        {
            get
            {
                if (this._xmlNode != null)
                {
                    return this._xmlNode.InnerXml;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the concatenated values of the node and all its child nodes.
        /// </summary>
        public string InnerText
        {
            get
            {
                if (this._xmlNode != null)
                {
                    return this._xmlNode.InnerText;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current XmlNode.
        /// </summary>
        public XmlNode XmlNode
        {
            get
            {
                return this._xmlNode;
            }
        }

        /// <summary>
        /// Loads the XML document from the specified string.
        /// </summary>
        /// <param name="xml">String containing the XML document to load.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml LoadXml(string xml)
        {
            return new FluentXml(xml, true);
        }

        /// <summary>
        /// Loads the XML document from the specified URL.
        /// </summary>
        /// <param name="filename">URL for the file containing the XML document to load.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml Load(string filename)
        {
            return new FluentXml(filename, false);
        }

        /// <summary>
        /// Loads the XML document from the specified stream.
        /// </summary>
        /// <param name="inStream">The stream containing the XML document to load.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml Load(Stream inStream)
        {
            return new FluentXml(inStream);
        }

        /// <summary>
        /// Loads the XML document from the specified System.IO.TextReader.
        /// </summary>
        /// <param name="txtReader">The TextReader used to feed the XML data.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml Load(TextReader txtReader)
        {
            return new FluentXml(txtReader);
        }

        /// <summary>
        /// Loads the XML document from the specified XmlNode.
        /// </summary>
        /// <param name="xmlNode">The XmlNode used to feed the XML data.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml Load(XmlNode xmlNode)
        {
            return new FluentXml(xmlNode);
        }

        /// <summary>
        /// Loads the XML document from the specified System.Xml.XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader used to feed the XML data.</param>
        /// <returns>FluentXml instance.</returns>
        public static FluentXml Load(XmlReader reader)
        {
            return new FluentXml(reader);
        }

        /// <summary>
        /// Gets the node with the specified name.
        /// </summary>
        /// <param name="name">The node name.</param>
        /// <returns>FluentXml instance.</returns>
        public FluentXml Node(string name)
        {
            if (!this.IsNullOrWhiteSpace(name))
            {
                var xmlNode = this._xmlNode.SelectSingleNode("descendant-or-self::" + name);
                return new FluentXml(xmlNode);
            }

            return this;
        }

        /// <summary>
        /// Gets the value of the current node.
        /// </summary>
        /// <returns>The value of current node.</returns>
        public string Value()
        {
            if (this._xmlNode != null)
            {
                return this._xmlNode.InnerText;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of the current node.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <returns>The value.</returns>
        public T Value<T>()
        {
            if (this._xmlNode != null)
            {
                return XmlConverter.ToObject<T>(this._xmlNode.InnerText);
            }

            return default(T);
        }

        /// <summary>
        /// Gets the value of the current node.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="defaultOnError">When convert failed, true to return a default value of targetType; false to return defaultValue.</param>
        /// <returns>The value.</returns>
        public T Value<T>(T defaultValue, bool defaultOnError = false)
        {
            if (this._xmlNode != null)
            {
                return XmlConverter.ToObject<T>(this._xmlNode.InnerText, defaultValue, defaultOnError);
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets the value of the current node.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>FluentXml instance.</returns>
        public FluentXml Value(string value)
        {
            if (this._xmlNode != null)
            {
                this._xmlNode.InnerText = value;
            }

            return this;
        }

        /// <summary>
        /// Gets the nodes with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>List of FluentXml.</returns>
        public List<FluentXml> Nodes(string name = null)
        {
            List<FluentXml> result = new List<FluentXml>();

            XmlNodeList nodes =
                this.IsNullOrWhiteSpace(name)
                ? this._xmlNode.ChildNodes
                : this._xmlNode.SelectNodes("descendant-or-self::" + name);

            foreach (XmlNode node in nodes)
            {
                result.Add(new FluentXml(node));
            }

            return result;
        }

        /// <summary>
        /// Adds a node.
        /// </summary>
        /// <param name="name">The name of node to add.</param>
        /// <param name="value">The value of the node.</param>
        /// <returns>FluentXml instance.</returns>
        public FluentXml AddNode(string name, string value = null)
        {
            if (this.IsNullOrWhiteSpace(name))
            {
                return this;
            }

            string xmlns = null;
            var parent = this._xmlNode;

            while (parent != null)
            {
                if (!this.IsNullOrWhiteSpace(parent.NamespaceURI))
                {
                    xmlns = parent.NamespaceURI;
                    break;
                }
                else
                {
                    parent = parent.ParentNode;
                }
            }

            XmlNode child;

            if (xmlns == null)
            {
                child = this._xmlNode.OwnerDocument.CreateElement(name);
            }
            else
            {
                child = this._xmlNode.OwnerDocument.CreateElement(name, xmlns);
            }

            if (!string.IsNullOrEmpty(value))
            {
                child.InnerText = value;
            }

            this._xmlNode.AppendChild(child);

            return this;
        }

        /// <summary>
        /// Gets the attribute with the specified attribute name.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The attribute value.</returns>
        public string Attribute(string attribute)
        {
            if (this.IsNullOrWhiteSpace(attribute)
                || this._xmlNode == null
                || this._xmlNode.Attributes == null
                || this._xmlNode.Attributes[attribute] == null)
            {
                return null;
            }

            return this._xmlNode.Attributes[attribute].Value;
        }

        /// <summary>
        /// Gets the attribute with the specified attribute name.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The attribute value.</returns>
        public T Attribute<T>(string attribute)
        {
            if (this.IsNullOrWhiteSpace(attribute)
                || this._xmlNode == null
                || this._xmlNode.Attributes == null
                || this._xmlNode.Attributes[attribute] == null)
            {
                return default(T);
            }

            return XmlConverter.ToObject<T>(this._xmlNode.Attributes[attribute].Value);
        }

        /// <summary>
        /// Gets the attribute with the specified attribute name.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="attribute">The attribute name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="defaultOnError">When convert failed, true to return a default value of targetType; false to return defaultValue.</param>
        /// <returns>The value.</returns>
        public T Attribute<T>(string attribute, T defaultValue, bool defaultOnError = false)
        {
            if (this.IsNullOrWhiteSpace(attribute)
                || this._xmlNode == null
                || this._xmlNode.Attributes == null
                || this._xmlNode.Attributes[attribute] == null)
            {
                return defaultValue;
            }

            return XmlConverter.ToObject<T>(this._xmlNode.Attributes[attribute].Value, defaultValue, defaultOnError);
        }

        /// <summary>
        /// Sets the attribute value with the specified attribute name.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>FluentXml instance.</returns>
        public FluentXml Attribute(string attribute, string value)
        {
            if (this._xmlNode != null)
            {
                var xmlAttribute = this._xmlNode.Attributes[attribute];

                if (xmlAttribute == null)
                {
                    xmlAttribute = this._xmlNode.OwnerDocument.CreateAttribute(attribute);
                    this._xmlNode.Attributes.Append(xmlAttribute);
                }

                xmlAttribute.Value = value;
            }

            return this;
        }

        /// <summary>
        /// Gets the node with xml path.
        /// </summary>
        /// <example>xxx.NodePath("A.B.C") or xxx.NodePath("A.B.C.D[E]")</example>
        /// <param name="xmlPath">The XML path.</param>
        /// <returns>FluentXml instance.</returns>
        public FluentXml NodePath(string xmlPath)
        {
            if (this.IsNullOrWhiteSpace(xmlPath))
            {
                return this;
            }

            if (!xmlPath.Contains("."))
            {
                return this.GetNodeWithIndex(xmlPath);
            }
            else
            {
                var propertyChain = xmlPath.Trim().Split('.');

                FluentXml result = this;

                foreach (var propertyName in propertyChain)
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        result = result.Node(propertyName);
                    }
                    else
                    {
                        return result.GetNodeWithIndex(propertyName);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the nodes with xml path.
        /// </summary>
        /// <example>example: xxx.NodesPath("A.B.C")</example>
        /// <param name="xmlPath">The XML path.</param>
        /// <returns>List of FluentXml.</returns>
        public List<FluentXml> NodesPath(string xmlPath)
        {
            if (this.IsNullOrWhiteSpace(xmlPath))
            {
                return this.Nodes();
            }

            if (!xmlPath.Contains("."))
            {
                return this.GetNodesWithIndex(xmlPath);
            }
            else
            {
                var propertyChain = xmlPath.Trim().Split('.');

                FluentXml result = this;

                for (int i = 0; i < propertyChain.Length - 1; i++)
                {
                    result = result.Node(propertyChain[i]);
                }

                return result.GetNodesWithIndex(propertyChain[propertyChain.Length - 1]);
            }
        }

        /// <summary>
        /// Gets the node value with the xml path.
        /// </summary>
        /// <example>example: xxx.NodePathValue("A.B.C") or xxx.NodePathValue("A.B.C[D]")</example>
        /// <param name="xmlPath">The XML path.</param>
        /// <returns>The value.</returns>
        public string NodePathValue(string xmlPath)
        {
            if (this.IsNullOrWhiteSpace(xmlPath))
            {
                return null;
            }

            if (!xmlPath.Contains("."))
            {
                return this.GetValueWithIndex(xmlPath);
            }
            else
            {
                var propertyChain = xmlPath.Trim().Split('.');

                FluentXml result = this;

                foreach (var propertyName in propertyChain)
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        result = result.Node(propertyName);
                    }
                    else
                    {
                        return result.GetValueWithIndex(propertyName);
                    }
                }

                return result.Value();
            }
        }

        /// <summary>
        /// Gets the node value with the xml path.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="xmlPath">The XML path.</param>
        /// <returns>The value.</returns>
        public T NodePathValue<T>(string xmlPath)
        {
            if (this.IsNullOrWhiteSpace(xmlPath))
            {
                return default(T);
            }

            if (!xmlPath.Contains("."))
            {
                return XmlConverter.ToObject<T>(this.GetValueWithIndex(xmlPath));
            }
            else
            {
                var propertyChain = xmlPath.Trim().Split('.');

                FluentXml result = this;

                foreach (var propertyName in propertyChain)
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        result = result.Node(propertyName);
                    }
                    else
                    {
                        return XmlConverter.ToObject<T>(result.GetValueWithIndex(propertyName));
                    }
                }

                return XmlConverter.ToObject<T>(result.Value());
            }
        }

        /// <summary>
        /// Gets the node value with the xml path.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="defaultOnError">When convert failed, true to return a default value of targetType; false to return defaultValue.</param>
        /// <returns>The value.</returns>
        public T NodePathValue<T>(string xmlPath, T defaultValue, bool defaultOnError = false)
        {
            if (this.IsNullOrWhiteSpace(xmlPath))
            {
                return defaultValue;
            }

            if (!xmlPath.Contains("."))
            {
                return XmlConverter.ToObject<T>(this.GetValueWithIndex(xmlPath), defaultValue, defaultOnError);
            }
            else
            {
                var propertyChain = xmlPath.Trim().Split('.');

                FluentXml result = this;

                foreach (var propertyName in propertyChain)
                {
                    if (!propertyName.EndsWith("]"))
                    {
                        result = result.Node(propertyName);
                    }
                    else
                    {
                        return XmlConverter.ToObject<T>(result.GetValueWithIndex(propertyName), defaultValue, defaultOnError);
                    }
                }

                return XmlConverter.ToObject<T>(result.Value(), defaultValue, defaultOnError);
            }
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        private bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the value of node name with indexer.
        /// </summary>
        /// <param name="propertyNameWithIndex">The node name with indexer.</param>
        /// <returns>The value.</returns>
        private string GetValueWithIndex(string propertyNameWithIndex)
        {
            if (!propertyNameWithIndex.EndsWith("]"))
            {
                return this.Node(propertyNameWithIndex).Value();
            }
            else
            {
                int propertyNameIndex = propertyNameWithIndex.LastIndexOf("[");
                string nodeName = propertyNameWithIndex.Substring(0, propertyNameIndex);
                string attributeName = propertyNameWithIndex.Substring(propertyNameIndex, propertyNameWithIndex.Length - propertyNameIndex).Trim('[', ']');

                return this.Node(nodeName).Attribute(attributeName);
            }
        }

        /// <summary>
        /// Gets the node of the name with indexer.
        /// </summary>
        /// <param name="propertyNameWithIndex">The node name with indexer.</param>
        /// <returns>FluentXml instance.</returns>
        private FluentXml GetNodeWithIndex(string propertyNameWithIndex)
        {
            if (!propertyNameWithIndex.EndsWith("]"))
            {
                return this.Node(propertyNameWithIndex);
            }
            else
            {
                int propertyNameIndex = propertyNameWithIndex.LastIndexOf("[");
                string nodeName = propertyNameWithIndex.Substring(0, propertyNameIndex);
                string attributeName = propertyNameWithIndex.Substring(propertyNameIndex, propertyNameWithIndex.Length - propertyNameIndex).Trim('[', ']');

                return this.Node(nodeName);
            }
        }

        /// <summary>
        /// Gets the nodes of the name with indexer.
        /// </summary>
        /// <param name="propertyNameWithIndex">The node name with indexer.</param>
        /// <returns>List of FluentXml.</returns>
        private List<FluentXml> GetNodesWithIndex(string propertyNameWithIndex)
        {
            if (!propertyNameWithIndex.EndsWith("]"))
            {
                return this.Nodes(propertyNameWithIndex);
            }
            else
            {
                int propertyNameIndex = propertyNameWithIndex.LastIndexOf("[");
                string nodeName = propertyNameWithIndex.Substring(0, propertyNameIndex);
                string attributeName = propertyNameWithIndex.Substring(propertyNameIndex, propertyNameWithIndex.Length - propertyNameIndex).Trim('[', ']');

                return this.Nodes(nodeName);
            }
        }
    }
}