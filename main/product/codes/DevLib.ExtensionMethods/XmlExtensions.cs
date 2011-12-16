//-----------------------------------------------------------------------
// <copyright file="XmlExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Xml;

    /// <summary>
    /// Xml Extensions
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Appends a child to a XML node
        /// </summary>
        /// <param name="childNode">The name of the child node</param>
        /// <param name="sourceNode">The parent node</param>
        public static void AppendChildNodeTo(this string childNode, ref XmlNode sourceNode)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlNode node = document.CreateElement(childNode);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Appends a child to a XML node
        /// </summary>
        /// <param name="childNode">The name of the child node</param>
        /// <param name="sourceNode">The parent node</param>
        /// <param name="namespaceUri">The node namespace</param>
        public static void AppendChildNodeTo(this string childNode, ref XmlNode sourceNode, string namespaceUri)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlNode node = document.CreateElement(childNode, namespaceUri);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Appends a CData section to a XML node and prefills the provided data
        /// </summary>
        /// <param name="cData">The CData section value</param>
        /// <param name="sourceNode">The parent node</param>
        public static void AppendCDataSectionTo(this string cData, XmlNode sourceNode)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlCDataSection node = document.CreateCDataSection(cData);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Returns the value of a nested CData section
        /// </summary>
        /// <param name="source">The parent node</param>
        /// <returns>The CData section content</returns>
        public static string GetCDataSection(this XmlNode source)
        {
            foreach (var node in source.ChildNodes)
            {
                if (node is XmlCDataSection)
                {
                    return ((XmlCDataSection)node).Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an attribute value
        /// </summary>
        /// <param name="source">The node to retreive the value from</param>
        /// <param name="attributeName">The Name of the attribute</param>
        /// <returns>The attribute value</returns>
        public static string GetAttribute(this XmlNode source, string attributeName)
        {
            return GetAttribute(source, attributeName, null);
        }

        /// <summary>
        /// Gets an attribute value
        /// If the value is empty, uses the specified default value
        /// </summary>
        /// <param name="source">The node to retreive the value from</param>
        /// <param name="attributeName">The Name of the attribute</param>
        /// <param name="defaultValue">The default value to be returned if no matching attribute exists</param>
        /// <returns>The attribute value</returns>
        public static string GetAttribute(this XmlNode source, string attributeName, string defaultValue)
        {
            XmlAttribute attribute = source.Attributes[attributeName];
            return attribute != null ? attribute.InnerText : defaultValue;
        }

        /// <summary>
        /// Gets an attribute value converted to the specified data type
        /// </summary>
        /// <typeparam name="T">The desired return data type</typeparam>
        /// <param name="source">The node to evaluate</param>
        /// <param name="attributeName">The Name of the attribute</param>
        /// <returns>The attribute value</returns>
        public static T GetAttribute<T>(this XmlNode source, string attributeName)
        {
            return GetAttribute<T>(source, attributeName, default(T));
        }

        /// <summary>
        /// Gets an attribute value converted to the specified data type
        /// If the value is empty, uses the specified default value
        /// </summary>
        /// <typeparam name="T">The desired return data type</typeparam>
        /// <param name="source">The node to evaluate</param>
        /// <param name="attributeName">The Name of the attribute</param>
        /// <param name="defaultValue">The default value to be returned if no matching attribute exists</param>
        /// <returns>The attribute value</returns>
        public static T GetAttribute<T>(this XmlNode source, string attributeName, T defaultValue)
        {
            var value = GetAttribute(source, attributeName);

            return !value.IsEmpty() ? value.ConvertTo<T>(defaultValue) : defaultValue;
        }

        /// <summary>
        /// Creates or updates an attribute with the passed object
        /// </summary>
        /// <param name="source">The node to evaluate</param>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value</param>
        public static void SetAttribute(this XmlNode source, string name, object value)
        {
            SetAttribute(source, name, value != null ? value.ToString() : null);
        }

        /// <summary>
        /// Creates or updates an attribute with the passed value
        /// </summary>
        /// <param name="source">The node to evaluate</param>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value</param>
        public static void SetAttribute(this XmlNode source, string name, string value)
        {
            if (source != null)
            {
                var attribute = source.Attributes[name, source.NamespaceURI];

                if (attribute == null)
                {
                    attribute = source.OwnerDocument.CreateAttribute(name, source.OwnerDocument.NamespaceURI);
                    source.Attributes.Append(attribute);
                }

                attribute.InnerText = value;
            }
        }
    }
}
