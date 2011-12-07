//-----------------------------------------------------------------------
// <copyright file="XMLViewer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.WinForms
{
    using System;
    using System.Drawing;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// RichTextBox as XMLViewer
    /// </summary>
    public class XMLViewer : System.Windows.Forms.RichTextBox
    {
        /// <summary>
        ///
        /// </summary>
        private XMLViewerSettings _settings;

        /// <summary>
        ///Gets or sets a value indicating whether
        /// </summary>
        public bool IsViewingXML
        {
            get;
            private set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        public string LastText
        {
            get;
            private set;
        }

        /// <summary>
        /// The format settings.
        /// </summary>
        public XMLViewerSettings Settings
        {
            get
            {
                if (this._settings == null)
                {
                    this._settings = new XMLViewerSettings
                    {
                        AttributeKey = Color.Red,
                        AttributeValue = Color.Blue,
                        Tag = Color.Blue,
                        Element = Color.DarkRed,
                        Value = Color.Black,
                    };
                }

                return this._settings;
            }

            set
            {
                this._settings = value;
            }
        }

        /// <summary>
        /// Convert the Xml to Rtf with some formats specified in the XMLViewerSettings,
        /// and then set the Rtf property to the value.
        /// </summary>
        /// <param name="includeDeclaration">
        /// Specify whether include the declaration.
        /// </param>
        public void Process(bool includeDeclaration)
        {
            try
            {
                // The Rtf contains 2 parts, header and content. The colortbl is a part of
                // the header, and the {1} will be replaced with the content.
                string rtfFormat = @"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033\deflangfe2052
{{\fonttbl{{\f0\fnil Fixedsys;}}}}
{{\colortbl ;{0}}}
\viewkind4\uc1\pard\lang1033\f0 
{1}}}";

                // Get the XDocument from the Text property.
                var xmlDoc = XDocument.Parse(this.Text, LoadOptions.None);

                StringBuilder xmlRtfContent = new StringBuilder();

                // If includeDeclaration is true and the XDocument has declaration,
                // then add the declaration to the content.
                if (includeDeclaration && xmlDoc.Declaration != null)
                {
                    // The constants in XMLViewerSettings are used to specify the order
                    // in colortbl of the Rtf.
                    xmlRtfContent.AppendFormat(@"
\cf{0} <?\cf{1} xml \cf{2} version\cf{0} =\cf0 ""\cf{3} {4}\cf0 "" 
\cf{2} encoding\cf{0} =\cf0 ""\cf{3} {5}\cf0 ""\cf{0} ?>\par",
                        XMLViewerSettings.TagID,
                        XMLViewerSettings.ElementID,
                        XMLViewerSettings.AttributeKeyID,
                        XMLViewerSettings.AttributeValueID,
                        xmlDoc.Declaration.Version,
                        xmlDoc.Declaration.Encoding);
                }

                // Get the Rtf of the root element.
                string rootRtfContent = ProcessElement(xmlDoc.Root, 0);

                xmlRtfContent.Append(rootRtfContent);

                this.LastText = this.Text;

                // Construct the completed Rtf, and set the Rtf property to this value.
                this.Rtf = string.Format(rtfFormat, Settings.ToRtfFormatString(),
                    xmlRtfContent.ToString());

                this.IsViewingXML = true;
            }
            catch (System.Xml.XmlException xmlException)
            {
                this.IsViewingXML = false;
                this.LastText = this.Text;
                throw new ApplicationException("Please check the input Xml. Error:" + xmlException.Message, xmlException);
            }
            catch
            {
                this.IsViewingXML = false;
                this.LastText = this.Text;
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void ResetXMLtoPlain()
        {
            this.Rtf = string.Empty;
            base.ResetText();
            this.Text = this.LastText;
        }

        /// <summary>
        ///
        /// </summary>
        public void ResetPlainText()
        {
            string lastText = this.Text;
            this.Rtf = string.Empty;
            this.ResetText();
            this.Text = lastText;
        }

        /// <summary>
        ///
        /// </summary>
        public override void ResetText()
        {
            base.ResetText();
            this.IsViewingXML = false;
            this.LastText = string.Empty;
        }

        // Get the Rtf of the xml element.

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private string ProcessElement(XElement element, int level)
        {
            // This viewer does not support the Xml file that has Namespace.
            if (!string.IsNullOrEmpty(element.Name.Namespace.NamespaceName))
            {
                throw new ApplicationException(
                    "This viewer does not support the Xml file that has Namespace.");
            }

            string elementRtfFormat = string.Empty;
            StringBuilder childElementsRtfContent = new StringBuilder();
            StringBuilder attributesRtfContent = new StringBuilder();

            // Construct the indent.
            string indent = new string(' ', 4 * level);

            // If the element has child elements or value, then add the element to the
            // Rtf. {{0}} will be replaced with the attributes and {{1}} will be replaced
            // with the child elements or value.
            if (element.HasElements || !string.IsNullOrWhiteSpace(element.Value))
            {
                elementRtfFormat = string.Format(@"
{0}\cf{1} <\cf{2} {3}{{0}}\cf{1} >\par
{{1}}
{0}\cf{1} </\cf{2} {3}\cf{1} >\par",
                    indent,
                    XMLViewerSettings.TagID,
                    XMLViewerSettings.ElementID,
                    element.Name);

                // Construct the Rtf of child elements.
                if (element.HasElements)
                {
                    foreach (var childElement in element.Elements())
                    {
                        string childElementRtfContent =
                            ProcessElement(childElement, level + 1);
                        childElementsRtfContent.Append(childElementRtfContent);
                    }
                }

                // If !string.IsNullOrWhiteSpace(element.Value), then construct the Rtf
                // of the value.
                else
                {
                    childElementsRtfContent.AppendFormat(@"{ 0}\cf{ 1} { 2}\par",
                        new string(' ', 4 * (level + 1)),
                        XMLViewerSettings.ValueID,
                        CharacterEncoder.Encode(element.Value.Trim()));
                }
            }

            // This element only has attributes. {{0}} will be replaced with the attributes.
            else
            {
                elementRtfFormat =
                    string.Format(@"
{0}\cf{1} <\cf{2} {3}{{0}}\cf{1} />\par",
                    indent,
                    XMLViewerSettings.TagID,
                    XMLViewerSettings.ElementID,
                    element.Name);
            }

            // Construct the Rtf of the attributes.
            if (element.HasAttributes)
            {
                foreach (XAttribute attribute in element.Attributes())
                {
                    string attributeRtfContent = string.Format(
                        @" \cf{ 0} { 3}\cf{ 1} =\cf0 ""\cf{ 2} { 4}\cf0 """,
                        XMLViewerSettings.AttributeKeyID,
                        XMLViewerSettings.TagID,
                        XMLViewerSettings.AttributeValueID,
                        attribute.Name,
                        CharacterEncoder.Encode(attribute.Value));
                    attributesRtfContent.Append(attributeRtfContent);
                }

                attributesRtfContent.Append(" ");
            }

            return string.Format(elementRtfFormat, attributesRtfContent,
                childElementsRtfContent);
        }

        /// <summary>
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            //
            // XMLViewer
            //
            this.WordWrap = false;
            this.ResumeLayout(false);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class CharacterEncoder
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="originalText"></param>
        /// <returns></returns>
        public static string Encode(string originalText)
        {
            if (string.IsNullOrWhiteSpace(originalText))
            {
                return string.Empty;
            }

            StringBuilder encodedText = new StringBuilder();
            for (int i = 0; i < originalText.Length; i++)
            {
                switch (originalText[i])
                {
                    case '"':
                        encodedText.Append("&quot;");
                        break;
                    case '&':
                        encodedText.Append(@"&amp;");
                        break;
                    case '\'':
                        encodedText.Append(@"&apos;");
                        break;
                    case '<':
                        encodedText.Append(@"&lt;");
                        break;
                    case '>':
                        encodedText.Append(@"&gt;");
                        break;

                    // The character '\' should be converted to @"\\" or "\\\\"
                    case '\\':
                        encodedText.Append(@"\\");
                        break;

                    // The character '{' should be converted to @"\{" or "\\{"
                    case '{':
                        encodedText.Append(@"\{");
                        break;

                    // The character '}' should be converted to @"\}" or "\\}"
                    case '}':
                        encodedText.Append(@"\}");
                        break;

                    default:
                        encodedText.Append(originalText[i]);
                        break;
                }
            }

            return encodedText.ToString();
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class XMLViewerSettings
    {
        /// <summary>
        ///
        /// </summary>
        public const int ElementID = 1;

        /// <summary>
        ///
        /// </summary>
        public const int ValueID = 2;

        /// <summary>
        ///
        /// </summary>
        public const int AttributeKeyID = 3;

        /// <summary>
        ///
        /// </summary>
        public const int AttributeValueID = 4;

        /// <summary>
        ///
        /// </summary>
        public const int TagID = 5;

        /// <summary>
        /// The color of an Xml element name.
        /// </summary>
        public Color Element
        {
            get;
            set;
        }

        /// <summary>
        /// The color of an Xml element value.
        /// </summary>
        public Color Value
        {
            get;
            set;
        }

        /// <summary>
        /// The color of an Attribute Key in Xml element.
        /// </summary>
        public Color AttributeKey
        {
            get;
            set;
        }

        /// <summary>
        /// The color of an Attribute Value in Xml element.
        /// </summary>
        public Color AttributeValue
        {
            get;
            set;
        }

        /// <summary>
        /// The color of the tags and operators like "<,/> and =".
        /// </summary>
        public Color Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Convert the settings to Rtf color definitions.
        /// </summary>
        public string ToRtfFormatString()
        {
            // The Rtf color definition format.
            string format = @"\red{0}\green{1}\blue{2};";

            StringBuilder rtfFormatString = new StringBuilder();

            rtfFormatString.AppendFormat(format, Element.R, Element.G, Element.B);
            rtfFormatString.AppendFormat(format, Value.R, Value.G, Value.B);
            rtfFormatString.AppendFormat(format, AttributeKey.R, AttributeKey.G, AttributeKey.B);
            rtfFormatString.AppendFormat(format, AttributeValue.R, AttributeValue.G, AttributeValue.B);
            rtfFormatString.AppendFormat(format, Tag.R, Tag.G, Tag.B);

            return rtfFormatString.ToString();
        }
    }
}
