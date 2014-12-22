//-----------------------------------------------------------------------
// <copyright file="XmlExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    /// <summary>
    /// Xml Extensions.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Field ReaderSettings.
        /// </summary>
        private static readonly XmlReaderSettings ReaderSettings;

        /// <summary>
        /// Field WriterSettings.
        /// </summary>
        private static readonly XmlWriterSettings WriterSettings;

        /// <summary>
        /// Initializes static members of the <see cref="XmlExtensions" /> class.
        /// </summary>
        static XmlExtensions()
        {
            ReaderSettings = new XmlReaderSettings();
            ReaderSettings.CheckCharacters = true;
            ReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            ReaderSettings.ProhibitDtd = false;
            ReaderSettings.IgnoreComments = true;
            ReaderSettings.IgnoreProcessingInstructions = true;
            ReaderSettings.IgnoreWhitespace = true;
            ReaderSettings.ValidationFlags = XmlSchemaValidationFlags.None;
            ReaderSettings.ValidationType = ValidationType.None;
            ReaderSettings.CloseInput = true;

            WriterSettings = new XmlWriterSettings();
            WriterSettings.Indent = true;
            WriterSettings.Encoding = new UTF8Encoding(false);
            WriterSettings.CloseOutput = true;
        }

        /// <summary>
        /// Determines whether the source string is valid Xml string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>true if string is valid Xml string; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static bool IsValidXml(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            string temp = source.Trim();

            if (temp[0] != '<' || temp[temp.Length - 1] != '>')
            {
                return false;
            }

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), ReaderSettings))
            {
                try
                {
                    while (xmlReader.Read())
                    {
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Converts valid Xml string to the indent Xml string.
        /// </summary>
        /// <param name="source">The source Xml string.</param>
        /// <returns>Indent Xml string.</returns>
        public static string ToIndentXml(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            if (!source.IsValidXml())
            {
                return source;
            }

            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                using (XmlWriter xmlWriter = XmlTextWriter.Create(stringBuilder, WriterSettings))
                {
                    XDocument.Parse(source).Save(xmlWriter);
                    xmlWriter.Flush();
                    return stringBuilder.ToString();
                }
            }
            catch
            {
                return source;
            }
        }

        /// <summary>
        /// Converts RTF string to the Xml syntax highlight RTF string.
        /// </summary>
        /// <param name="source">The source RTF string.</param>
        /// <param name="indentXml">true to indent Xml string; otherwise, keep the original string.</param>
        /// <param name="darkStyle">true to use dark style; otherwise, use light style.</param>
        /// <returns>The Xml syntax highlight RTF string.</returns>
        public static string ToXmlSyntaxHighlightRtf(this string source, bool indentXml = true, bool darkStyle = false)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            string tempRtf = indentXml ? source.ToIndentXml() : source;

            StringBuilder highlightStringBuilder = new StringBuilder(string.Empty);

            bool inTag = false;
            bool inTagName = false;
            bool inQuotes = false;
            bool inComment = false;

            for (int i = 0; i < tempRtf.Length; i++)
            {
                bool isAppended = false;

                if (inTagName)
                {
                    if (tempRtf[i] == ' ')
                    {
                        highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfAttributeNameColor(darkStyle));

                        inTagName = false;
                    }
                }
                else if (inTag)
                {
                    if (tempRtf[i] == '"')
                    {
                        if (inQuotes)
                        {
                            highlightStringBuilder.Append(tempRtf[i]);
                            highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfAttributeNameColor(darkStyle));

                            isAppended = true;
                            inQuotes = false;
                        }
                        else
                        {
                            highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfAttributeValueColor(darkStyle));

                            inQuotes = true;
                        }
                    }
                }

                if (tempRtf[i] == '<')
                {
                    if (!inComment)
                    {
                        inTag = true;

                        if (tempRtf[i + 1] == '!')
                        {
                            if ((tempRtf[i + 2] == '-') && (tempRtf[i + 3] == '-'))
                            {
                                highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfCommentColor(darkStyle));

                                inComment = true;
                            }
                            else
                            {
                                highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfTagNameColor(darkStyle));

                                inTagName = true;
                            }
                        }

                        if (!inComment)
                        {
                            highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfTagColor(darkStyle));
                            highlightStringBuilder.Append(tempRtf[i]);

                            isAppended = true;

                            if (tempRtf[i + 1] == '?' || tempRtf[i + 1] == '/')
                            {
                                i++;
                                highlightStringBuilder.Append(tempRtf[i]);
                            }

                            highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfTagNameColor(darkStyle));

                            inTagName = true;
                        }
                    }
                }

                bool isClosingTag = false;

                if (tempRtf[i] == '>')
                {
                    isClosingTag = true;
                }

                if (i < tempRtf.Length - 1)
                {
                    if (tempRtf[i + 1] == '>')
                    {
                        if (tempRtf[i] == '?' || tempRtf[i] == '/')
                        {
                            isClosingTag = true;
                        }
                    }
                }

                if (isClosingTag)
                {
                    if (inComment)
                    {
                        if (tempRtf[i - 1] == '-' && tempRtf[i - 2] == '-')
                        {
                            highlightStringBuilder.Append(tempRtf[i]);
                            highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfDefaultColor(darkStyle));

                            isAppended = true;
                            inComment = false;
                            inTag = false;
                        }
                    }

                    if (inTag)
                    {
                        highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfLightTagColor);

                        if ((tempRtf[i] == '/') || (tempRtf[i] == '?'))
                        {
                            highlightStringBuilder.Append(tempRtf[i++]);
                        }

                        highlightStringBuilder.Append(tempRtf[i]);
                        highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfDefaultColor(darkStyle));

                        isAppended = true;
                        inTagName = false;
                        inTag = false;
                    }
                }

                if (!isAppended)
                {
                    highlightStringBuilder.Append(tempRtf[i]);
                }
            }

            string result = highlightStringBuilder.ToString();

            int colorTableStartIndex = result.IndexOf("{\\colortbl;");

            if (colorTableStartIndex != -1)
            {
                int colorTableEndIndex = result.IndexOf('}', colorTableStartIndex);

                result = result.Remove(colorTableStartIndex, colorTableEndIndex - colorTableStartIndex);
                result = result.Insert(colorTableStartIndex, XmlSyntaxHighlightColor.ColorTable);
            }
            else
            {
                int rtfIndex = result.IndexOf("\\rtf");

                if (rtfIndex < 0)
                {
                    result = result.Insert(0, "{\\rtf\\ansi\\deff0" + XmlSyntaxHighlightColor.ColorTable);
                    result += "}";
                }
                else
                {
                    int insertIndex = result.IndexOf('{', rtfIndex);

                    if (insertIndex == -1)
                    {
                        insertIndex = result.IndexOf('}', rtfIndex) - 1;
                    }

                    result = result.Insert(insertIndex, XmlSyntaxHighlightColor.ColorTable);
                }
            }

            return result;
        }

        /// <summary>
        /// Appends a child to a XML node.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <param name="childNode">The name of the child node.</param>
        /// <returns>The newly created XML node.</returns>
        public static XmlNode CreateChildNode(this XmlNode source, string childNode)
        {
            XmlDocument document = source is XmlDocument ? (XmlDocument)source : source.OwnerDocument;
            XmlNode node = document.CreateElement(childNode);
            source.AppendChild(node);
            return node;
        }

        /// <summary>
        /// Appends a child to a XML node.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <param name="childNode">The name of the child node.</param>
        /// <param name="namespaceUri">The node namespace.</param>
        /// <returns>The newly created XML node.</returns>
        public static XmlNode CreateChildNode(this XmlNode source, string childNode, string namespaceUri)
        {
            XmlDocument document = source is XmlDocument ? (XmlDocument)source : source.OwnerDocument;
            XmlNode node = document.CreateElement(childNode, namespaceUri);
            source.AppendChild(node);
            return node;
        }

        /// <summary>
        /// Appends a CData section to a XML node.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <returns>The created CData Section.</returns>
        public static XmlCDataSection CreateCDataSection(this XmlNode source)
        {
            return source.CreateCDataSection(string.Empty);
        }

        /// <summary>
        /// Appends a CData section to a XML node and prefills the provided data.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <param name="cData">The CData section value.</param>
        /// <returns>The created CData Section.</returns>
        public static XmlCDataSection CreateCDataSection(this XmlNode source, string cData)
        {
            XmlDocument document = source is XmlDocument ? (XmlDocument)source : source.OwnerDocument;
            XmlCDataSection node = document.CreateCDataSection(cData);
            source.AppendChild(node);
            return node;
        }

        /// <summary>
        /// Appends a child to a XML node.
        /// </summary>
        /// <param name="childNode">The name of the child node.</param>
        /// <param name="sourceNode">The parent node.</param>
        public static void AppendChildNodeTo(this string childNode, XmlNode sourceNode)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlNode node = document.CreateElement(childNode);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Appends a child to a XML node.
        /// </summary>
        /// <param name="childNode">The name of the child node.</param>
        /// <param name="sourceNode">The parent node.</param>
        /// <param name="namespaceUri">The node namespace.</param>
        public static void AppendChildNodeTo(this string childNode, XmlNode sourceNode, string namespaceUri)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlNode node = document.CreateElement(childNode, namespaceUri);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Appends a CData section to a XML node and prefills the provided data.
        /// </summary>
        /// <param name="cData">The CData section value.</param>
        /// <param name="sourceNode">The parent node.</param>
        public static void AppendCDataSectionTo(this string cData, XmlNode sourceNode)
        {
            XmlDocument document = sourceNode is XmlDocument ? (XmlDocument)sourceNode : sourceNode.OwnerDocument;
            XmlCDataSection node = document.CreateCDataSection(cData);
            sourceNode.AppendChild(node);
        }

        /// <summary>
        /// Returns the value of a nested CData section.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <returns>The CData section content.</returns>
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
        /// Gets an attribute value.
        /// </summary>
        /// <param name="source">The node to retrieve the value from.</param>
        /// <param name="attributeName">The Name of the attribute.</param>
        /// <returns>The attribute value.</returns>
        public static string GetAttribute(this XmlNode source, string attributeName)
        {
            return GetAttribute(source, attributeName, null);
        }

        /// <summary>
        /// Gets an attribute value
        /// If the value is empty, uses the specified default value.
        /// </summary>
        /// <param name="source">The node to retrieve the value from.</param>
        /// <param name="attributeName">The Name of the attribute.</param>
        /// <param name="defaultValue">The default value to be returned if no matching attribute exists.</param>
        /// <returns>The attribute value.</returns>
        public static string GetAttribute(this XmlNode source, string attributeName, string defaultValue)
        {
            XmlAttribute attribute = source.Attributes[attributeName];
            return attribute != null ? attribute.InnerText : defaultValue;
        }

        /// <summary>
        /// Gets an attribute value converted to the specified data type.
        /// </summary>
        /// <typeparam name="T">The desired return data type.</typeparam>
        /// <param name="source">The node to evaluate.</param>
        /// <param name="attributeName">The Name of the attribute.</param>
        /// <returns>The attribute value.</returns>
        public static T GetAttribute<T>(this XmlNode source, string attributeName)
        {
            return GetAttribute<T>(source, attributeName, default(T));
        }

        /// <summary>
        /// Gets an attribute value converted to the specified data type
        /// If the value is empty, uses the specified default value.
        /// </summary>
        /// <typeparam name="T">The desired return data type.</typeparam>
        /// <param name="source">The node to evaluate.</param>
        /// <param name="attributeName">The Name of the attribute.</param>
        /// <param name="defaultValue">The default value to be returned if no matching attribute exists.</param>
        /// <returns>The attribute value.</returns>
        public static T GetAttribute<T>(this XmlNode source, string attributeName, T defaultValue)
        {
            var value = GetAttribute(source, attributeName);

            return !value.IsNullOrEmpty() ? value.ConvertTo<T>(defaultValue) : defaultValue;
        }

        /// <summary>
        /// Creates or updates an attribute with the passed object.
        /// </summary>
        /// <param name="source">The node to evaluate.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public static void SetAttribute(this XmlNode source, string name, object value)
        {
            SetAttribute(source, name, value != null ? value.ToString() : null);
        }

        /// <summary>
        /// Creates or updates an attribute with the passed value.
        /// </summary>
        /// <param name="source">The node to evaluate.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
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

    /// <summary>
    /// Xml syntax highlight color.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    internal static class XmlSyntaxHighlightColor
    {
        /// <summary>
        /// Represents light style RTF tag color.
        /// </summary>
        public const string RtfLightTagColor = "\\cf1 ";

        /// <summary>
        /// Represents light style RTF tag name color.
        /// </summary>
        public const string RtfLightTagNameColor = "\\cf2 ";

        /// <summary>
        /// Represents light style RTF attribute name color.
        /// </summary>
        public const string RtfLightAttributeNameColor = "\\cf3 ";

        /// <summary>
        /// Represents light style RTF attribute value color.
        /// </summary>
        public const string RtfLightAttributeValueColor = "\\cf4 ";

        /// <summary>
        /// Represents light style RTF comment color.
        /// </summary>
        public const string RtfLightCommentColor = "\\cf5 ";

        /// <summary>
        /// Represents light style RTF default color.
        /// </summary>
        public const string RtfLightDefaultColor = "\\cf6 ";

        /// <summary>
        /// Represents dark style RTF tag color.
        /// </summary>
        public const string RtfDarkTagColor = "\\cf7 ";

        /// <summary>
        /// Represents dark style RTF tag name color.
        /// </summary>
        public const string RtfDarkTagNameColor = "\\cf8 ";

        /// <summary>
        /// Represents dark style RTF attribute name color.
        /// </summary>
        public const string RtfDarkAttributeNameColor = "\\cf9 ";

        /// <summary>
        /// Represents dark style RTF attribute value color.
        /// </summary>
        public const string RtfDarkAttributeValueColor = "\\cf10 ";

        /// <summary>
        /// Represents dark style RTF comment color.
        /// </summary>
        public const string RtfDarkCommentColor = "\\cf11 ";

        /// <summary>
        /// Represents dark style RTF default color.
        /// </summary>
        public const string RtfDarkDefaultColor = "\\cf12 ";

        /// <summary>
        /// Represents color table.
        /// </summary>
        public static readonly string ColorTable;

        /// <summary>
        /// Field LightTagColor.
        /// </summary>
        private const string LightTagColor = "\\red0\\green0\\blue255";

        /// <summary>
        /// Field LightTagNameColor.
        /// </summary>
        private const string LightTagNameColor = "\\red163\\green21\\blue21";

        /// <summary>
        /// Field LightAttributeNameColor.
        /// </summary>
        private const string LightAttributeNameColor = "\\red253\\green52\\blue0";

        /// <summary>
        /// Field LightAttributeValueColor.
        /// </summary>
        private const string LightAttributeValueColor = "\\red0\\green0\\blue255";

        /// <summary>
        /// Field LightCommentTextColor.
        /// </summary>
        private const string LightCommentTextColor = "\\red0\\green128\\blue0";

        /// <summary>
        /// Field LightDefaultColor.
        /// </summary>
        private const string LightDefaultColor = "\\red0\\green0\\blue0";

        /// <summary>
        /// Field DarkTagColor.
        /// </summary>
        private const string DarkTagColor = "\\red64\\green196\\blue255";

        /// <summary>
        /// Field DarkTagNameColor.
        /// </summary>
        private const string DarkTagNameColor = "\\red64\\green196\\blue255";

        /// <summary>
        /// Field DarkAttributeNameColor.
        /// </summary>
        private const string DarkAttributeNameColor = "\\red237\\green218\\blue192";

        /// <summary>
        /// Field DarkAttributeValueColor.
        /// </summary>
        private const string DarkAttributeValueColor = "\\red255\\green128\\blue255";

        /// <summary>
        /// Field DarkCommentTextColor.
        /// </summary>
        private const string DarkCommentTextColor = "\\red0\\green128\\blue0";

        /// <summary>
        /// Field DarkDefaultColor.
        /// </summary>
        private const string DarkDefaultColor = "\\red225\\green225\\blue225";

        /// <summary>
        /// Initializes static members of the <see cref="XmlSyntaxHighlightColor" /> class.
        /// </summary>
        static XmlSyntaxHighlightColor()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("{\\colortbl");
            stringBuilder.Append(";");
            stringBuilder.Append(LightTagColor);
            stringBuilder.Append(";");
            stringBuilder.Append(LightTagNameColor);
            stringBuilder.Append(";");
            stringBuilder.Append(LightAttributeNameColor);
            stringBuilder.Append(";");
            stringBuilder.Append(LightAttributeValueColor);
            stringBuilder.Append(";");
            stringBuilder.Append(LightCommentTextColor);
            stringBuilder.Append(";");
            stringBuilder.Append(LightDefaultColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkTagColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkTagNameColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkAttributeNameColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkAttributeValueColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkCommentTextColor);
            stringBuilder.Append(";");
            stringBuilder.Append(DarkDefaultColor);
            stringBuilder.Append(";}");

            ColorTable = stringBuilder.ToString();
        }

        /// <summary>
        /// Get RTF tag color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF tag color.</returns>
        public static string RtfTagColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkTagColor : RtfLightTagColor;
        }

        /// <summary>
        /// Get RTF tag name color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF tag name color.</returns>
        public static string RtfTagNameColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkTagNameColor : RtfLightTagNameColor;
        }

        /// <summary>
        /// Get RTF attribute name color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF attribute name color.</returns>
        public static string RtfAttributeNameColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkAttributeNameColor : RtfLightAttributeNameColor;
        }

        /// <summary>
        /// Get RTF attribute value color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF attribute value color.</returns>
        public static string RtfAttributeValueColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkAttributeValueColor : RtfLightAttributeValueColor;
        }

        /// <summary>
        /// Get RTF comment color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF comment color.</returns>
        public static string RtfCommentColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkCommentColor : RtfLightCommentColor;
        }

        /// <summary>
        /// Get RTF default color.
        /// </summary>
        /// <param name="darkStyle">true to use dark style, otherwise, use light style.</param>
        /// <returns>RTF default color.</returns>
        public static string RtfDefaultColor(bool darkStyle = false)
        {
            return darkStyle ? RtfDarkDefaultColor : RtfLightDefaultColor;
        }
    }
}
