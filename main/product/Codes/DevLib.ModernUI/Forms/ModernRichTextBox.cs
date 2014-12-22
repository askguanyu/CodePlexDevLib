//-----------------------------------------------------------------------
// <copyright file="ModernRichTextBox.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Schema;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernTextBox user control.
    /// </summary>
    [ToolboxBitmap(typeof(RichTextBox))]
    public class ModernRichTextBox : Control, IModernControl
    {
        /// <summary>
        /// Field ReaderSettings.
        /// </summary>
        private static readonly XmlReaderSettings ReaderSettings;

        /// <summary>
        /// Field _highlightSyncRoot
        /// </summary>
        private readonly object _highlightSyncRoot = new object();

        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _modernFontSize.
        /// </summary>
        private ModernFontSize _modernFontSize = ModernFontSize.Small;

        /// <summary>
        /// Field _modernFontWeight.
        /// </summary>
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Regular;

        /// <summary>
        /// Field _baseRichTextBox.
        /// </summary>
        private PromptedRichTextBox _baseRichTextBox;

        /// <summary>
        /// Field _indentXml.
        /// </summary>
        private bool _indentXml = true;

        /// <summary>
        /// Field _omitXmlDeclaration.
        /// </summary>
        private bool _omitXmlDeclaration = true;

        /// <summary>
        /// Field _highlightXmlSyntax.
        /// </summary>
        private bool _highlightXmlSyntax = true;

        /// <summary>
        /// Initializes static members of the <see cref="ModernRichTextBox" /> class.
        /// </summary>
        static ModernRichTextBox()
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernRichTextBox" /> class.
        /// </summary>
        public ModernRichTextBox()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

            base.TabStop = false;
            this.GotFocus += this.OnModernTextBoxGotFocus;

            this.CreateBaseTextBox();
            this.UpdateBaseTextBox();
            this.AddEventHandler();

            this.FontSize = ModernFontSize.Small;
            this.FontWeight = ModernFontWeight.Regular;
            this.WordWrap = false;
            this.MaxLength = int.MaxValue;
            this.ScrollBars = RichTextBoxScrollBars.Both;
            this.OmitXmlDeclaration = true;
        }

        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

        /// <summary>
        /// Event AcceptsTabChanged.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler AcceptsTabChanged;

        /// <summary>
        /// Event Pasted.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ClipboardEventArgs> Pasted;

        /// <summary>
        /// Gets or sets a value indicating whether indent Xml string.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool IndentXml
        {
            get
            {
                return this._indentXml;
            }

            set
            {
                if (this._indentXml != value)
                {
                    this._indentXml = value;
                    this.UpdateXmlSyntaxHighlight(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether indent Xml string.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool OmitXmlDeclaration
        {
            get
            {
                return this._omitXmlDeclaration;
            }

            set
            {
                if (this._omitXmlDeclaration != value)
                {
                    this._omitXmlDeclaration = value;
                    this.UpdateXmlSyntaxHighlight(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether highlight Xml syntax.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool HighlightXmlSyntax
        {
            get
            {
                return this._highlightXmlSyntax;
            }

            set
            {
                if (this._highlightXmlSyntax != value)
                {
                    this._highlightXmlSyntax = value;
                    this.UpdateXmlSyntaxHighlight(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the System.Windows.Forms.RichTextBox control, including all rich text format (RTF) codes.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Rtf
        {
            get
            {
                return this._baseRichTextBox.Rtf;
            }

            set
            {
                this._baseRichTextBox.Rtf = value;
            }
        }

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernColorStyle.Default)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernColorStyle ColorStyle
        {
            get
            {
                if (this.DesignMode || this._modernColorStyle != ModernColorStyle.Default)
                {
                    return this._modernColorStyle;
                }

                if (this.StyleManager != null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return this.StyleManager.ColorStyle;
                }

                if (this.StyleManager == null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return ModernConstants.DefaultColorStyle;
                }

                return this._modernColorStyle;
            }

            set
            {
                this._modernColorStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernThemeStyle.Default)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernThemeStyle ThemeStyle
        {
            get
            {
                if (this.DesignMode || this._modernThemeStyle != ModernThemeStyle.Default)
                {
                    return this._modernThemeStyle;
                }

                if (this.StyleManager != null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return this.StyleManager.ThemeStyle;
                }

                if (this.StyleManager == null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return ModernConstants.DefaultThemeStyle;
                }

                return this._modernThemeStyle;
            }

            set
            {
                this._modernThemeStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom BackColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseSelectable
        {
            get
            {
                return this.GetStyle(ControlStyles.Selectable);
            }

            set
            {
                this.SetStyle(ControlStyles.Selectable, value);
            }
        }

        /// <summary>
        /// Gets or sets modern font size.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontSize.Small)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontSize FontSize
        {
            get
            {
                return this._modernFontSize;
            }

            set
            {
                this._modernFontSize = value;
                this.UpdateBaseTextBox();
            }
        }

        /// <summary>
        /// Gets or sets modern font weight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Regular)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontWeight FontWeight
        {
            get
            {
                return this._modernFontWeight;
            }

            set
            {
                this._modernFontWeight = value;
                this.UpdateBaseTextBox();
            }
        }

        /// <summary>
        /// Gets or sets prompt text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue("")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category(ModernConstants.PropertyCategoryName)]
        public string PromptText
        {
            get
            {
                return this._baseRichTextBox.PromptText;
            }

            set
            {
                this._baseRichTextBox.PromptText = value;
            }
        }

        /// <summary>
        /// Gets or sets the shortcut menu associated with the control.
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get
            {
                return this._baseRichTextBox.ContextMenu;
            }

            set
            {
                base.ContextMenu = value;
                this._baseRichTextBox.ContextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets the ContextMenuStrip associated with this control.
        /// </summary>
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this._baseRichTextBox.ContextMenuStrip;
            }

            set
            {
                base.ContextMenuStrip = value;
                this._baseRichTextBox.ContextMenuStrip = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is a multiline control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool Multiline
        {
            get
            {
                return this._baseRichTextBox.Multiline;
            }

            set
            {
                this._baseRichTextBox.Multiline = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a multiline text box control automatically wraps words to the beginning of the next line when necessary.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool WordWrap
        {
            get
            {
                return this._baseRichTextBox.WordWrap;
            }

            set
            {
                this._baseRichTextBox.WordWrap = value;
            }
        }

        /// <summary>
        /// Gets or sets the current text in the TextBox.
        /// </summary>
        public override string Text
        {
            get
            {
                return this._baseRichTextBox.Text;
            }

            set
            {
                this._baseRichTextBox.Text = value;
                this.UpdateXmlSyntaxHighlight();
            }
        }

        /// <summary>
        /// Gets or sets the lines of text in a text box control.
        /// </summary>
        public string[] Lines
        {
            get
            {
                return this._baseRichTextBox.Lines;
            }

            set
            {
                this._baseRichTextBox.Lines = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the currently selected text in the control.
        /// </summary>
        [Browsable(false)]
        public string SelectedText
        {
            get
            {
                return this._baseRichTextBox.SelectedText;
            }

            set
            {
                this._baseRichTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether text in the text box is read-only.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return this._baseRichTextBox.ReadOnly;
            }

            set
            {
                this._baseRichTextBox.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can give the focus to this control using the TAB key.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        public new bool TabStop
        {
            get
            {
                return this._baseRichTextBox.TabStop;
            }

            set
            {
                this._baseRichTextBox.TabStop = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters the user can type or paste into the text box control.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return this._baseRichTextBox.MaxLength;
            }

            set
            {
                this._baseRichTextBox.MaxLength = value;
            }
        }

        /// <summary>
        /// Gets or sets which scroll bars should appear in a multiline control.
        /// </summary>
        public RichTextBoxScrollBars ScrollBars
        {
            get
            {
                return this._baseRichTextBox.ScrollBars;
            }

            set
            {
                this._baseRichTextBox.ScrollBars = value;
            }
        }

        /// <summary>
        /// Determines whether the source string is valid Xml string.
        /// </summary>
        /// <param name="xml">The source string.</param>
        /// <returns>true if string is valid Xml string; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public bool IsValidXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            string temp = xml.Trim();

            if (temp[0] != '<' || temp[temp.Length - 1] != '>')
            {
                return false;
            }

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml), ReaderSettings))
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
        /// <param name="xml">The source Xml string.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <returns>Indent Xml string.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public string ToIndentXml(string xml, bool omitXmlDeclaration)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            if (!this.IsValidXml(xml))
            {
                return xml;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                MemoryStream memoryStream = new MemoryStream();
                StreamReader streamReader = new StreamReader(memoryStream);

                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings { OmitXmlDeclaration = omitXmlDeclaration, Indent = true, Encoding = new UTF8Encoding(false), CloseOutput = true }))
                {
                    xmlDocument.Save(xmlWriter);
                    xmlWriter.Flush();
                    memoryStream.Position = 0;

                    return streamReader.ReadToEnd();
                }
            }
            catch
            {
                return xml;
            }
        }

        /// <summary>
        /// Converts RTF string to the Xml syntax highlight RTF string.
        /// </summary>
        /// <param name="rtf">The source RTF string.</param>
        /// <param name="indentXml">true to indent Xml string; otherwise, keep the original string.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="darkStyle">true to use dark style; otherwise, use light style.</param>
        /// <returns>The Xml syntax highlight RTF string.</returns>
        public string ToXmlSyntaxHighlightRtf(string rtf, bool indentXml = true, bool omitXmlDeclaration = false, bool darkStyle = false)
        {
            if (string.IsNullOrEmpty(rtf))
            {
                return rtf;
            }

            string tempRtf = indentXml ? this.ToIndentXml(rtf, omitXmlDeclaration) : rtf;

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
                        highlightStringBuilder.Append(XmlSyntaxHighlightColor.RtfTagColor(darkStyle));

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
        /// Highlights the Xml syntax for RichTextBox.
        /// </summary>
        /// <param name="indentXml">true to indent Xml string; otherwise, keep the original string.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="darkStyle">true to use dark style; otherwise, use light style.</param>
        public void HighlightXml(bool indentXml = true, bool omitXmlDeclaration = false, bool darkStyle = false)
        {
            if (!this.IsValidXml(this._baseRichTextBox.Text))
            {
                return;
            }

            if (indentXml)
            {
                this._baseRichTextBox.Text = this.ToIndentXml(this._baseRichTextBox.Text, omitXmlDeclaration);
            }

            this._baseRichTextBox.Rtf = this.ToXmlSyntaxHighlightRtf(this._baseRichTextBox.Rtf, indentXml, omitXmlDeclaration, darkStyle);
            this._baseRichTextBox.Tag = this._baseRichTextBox.Rtf;
        }

        /// <summary>
        /// Selects a range of text in the text box.
        /// </summary>
        /// <param name="start">The position of the first character in the current text selection within the text box.</param>
        /// <param name="length">The number of characters to select.</param>
        public void Select(int start, int length)
        {
            this._baseRichTextBox.Select(start, length);
        }

        /// <summary>
        /// Selects all text in the text box.
        /// </summary>
        public void SelectAll()
        {
            this._baseRichTextBox.SelectAll();
        }

        /// <summary>
        /// Clears all text from the text box control.
        /// </summary>
        public void Clear()
        {
            this._baseRichTextBox.Clear();
        }

        /// <summary>
        /// Appends text to the current text of a text box.
        /// </summary>
        /// <param name="text">The text to append to the current contents of the text box.</param>
        public void AppendText(string text)
        {
            this._baseRichTextBox.AppendText(text);
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.UpdateBaseTextBox();
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintBackground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintBackground != null)
            {
                this.CustomPaintBackground(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaint" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaint(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaint != null)
            {
                this.CustomPaint(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintForeground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintForeground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintForeground != null)
            {
                this.CustomPaintForeground(this, e);
            }
        }

        /// <summary>
        /// OnPaintBackground method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                Color backColor = this.BackColor;

                if (!this.UseCustomBackColor)
                {
                    backColor = ModernPaint.BackColor.Button.Normal(this.ThemeStyle);
                }

                this._baseRichTextBox.BackColor = backColor;

                if (backColor.A == 255)
                {
                    e.Graphics.Clear(backColor);
                    return;
                }

                base.OnPaintBackground(e);

                this.OnCustomPaintBackground(new ModernPaintEventArgs(backColor, Color.Empty, e.Graphics));
            }
            catch
            {
                this.Invalidate();
            }
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (this.GetStyle(ControlStyles.AllPaintingInWmPaint))
                {
                    this.OnPaintBackground(e);
                }

                this.OnCustomPaint(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
                this.OnPaintForeground(e);
            }
            catch
            {
                this.Invalidate();
            }
        }

        /// <summary>
        /// OnPaintForeground method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            if (!this.UseCustomForeColor)
            {
                this._baseRichTextBox.ForeColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);

                if (this.HighlightXmlSyntax)
                {
                    this.UpdateXmlSyntaxHighlight();
                    this._baseRichTextBox.ForeColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);
                }
            }
            else
            {
                this._baseRichTextBox.ForeColor = this.ForeColor;
            }

            Color borderColor = ModernPaint.BorderColor.Button.Normal(this.ThemeStyle);

            if (this.UseStyleColors)
            {
                borderColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }

            using (Pen p = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        /// <summary>
        /// OnResize method.
        /// </summary>
        /// <param name="e">EventArgs instance.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.UpdateBaseTextBox();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.V) ||
                (e.Control && e.KeyCode == Keys.V) ||
                (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert) ||
                (e.Shift && e.KeyCode == Keys.Insert))
            {
                if (this.Pasted != null)
                {
                    this.Pasted(this, new ClipboardEventArgs(Clipboard.GetText()));
                }

                this.UpdateXmlSyntaxHighlight();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.V) ||
                (e.Control && e.KeyCode == Keys.V) ||
                (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert) ||
                (e.Shift && e.KeyCode == Keys.Insert))
            {
                this.UpdateXmlSyntaxHighlight();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs" /> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == 22)
            {
                this.UpdateXmlSyntaxHighlight();
            }
        }

        /// <summary>
        /// OnModernTextBoxGotFocus method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void OnModernTextBoxGotFocus(object sender, EventArgs e)
        {
            this._baseRichTextBox.Focus();
        }

        /// <summary>
        /// BaseRichTextBoxAcceptsTabChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxAcceptsTabChanged(object sender, EventArgs e)
        {
            if (this.AcceptsTabChanged != null)
            {
                this.AcceptsTabChanged(this, e);
            }
        }

        /// <summary>
        /// BaseRichTextBoxSizeChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxSizeChanged(object sender, EventArgs e)
        {
            this.OnSizeChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxCursorChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxCursorChanged(object sender, EventArgs e)
        {
            this.OnCursorChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxContextMenuStripChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxContextMenuStripChanged(object sender, EventArgs e)
        {
            this.OnContextMenuStripChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxContextMenuChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxContextMenuChanged(object sender, EventArgs e)
        {
            this.OnContextMenuChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxClientSizeChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxClientSizeChanged(object sender, EventArgs e)
        {
            this.OnClientSizeChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxClick method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxClick(object sender, EventArgs e)
        {
            this.OnClick(e);
        }

        /// <summary>
        /// BaseRichTextBoxChangeUICues method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">UICuesEventArgs instance.</param>
        private void BaseRichTextBoxChangeUICues(object sender, UICuesEventArgs e)
        {
            this.OnChangeUICues(e);
        }

        /// <summary>
        /// BaseRichTextBoxCausesValidationChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxCausesValidationChanged(object sender, EventArgs e)
        {
            this.OnCausesValidationChanged(e);
        }

        /// <summary>
        /// BaseRichTextBoxKeyUp method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyEventArgs instance.</param>
        private void BaseRichTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        /// <summary>
        /// BaseRichTextBoxKeyPress method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyPressEventArgs instance.</param>
        private void BaseRichTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        /// <summary>
        /// BaseRichTextBoxKeyDown method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyEventArgs instance.</param>
        private void BaseRichTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        /// <summary>
        /// BaseRichTextBoxTextChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseRichTextBoxTextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);
        }

        /// <summary>
        /// Create base text box.
        /// </summary>
        private void CreateBaseTextBox()
        {
            if (this._baseRichTextBox != null)
            {
                return;
            }

            this._baseRichTextBox = new PromptedRichTextBox();

            this._baseRichTextBox.BorderStyle = BorderStyle.None;
            this._baseRichTextBox.Font = ModernFonts.TextBox(this._modernFontSize, this._modernFontWeight);
            this._baseRichTextBox.Location = new Point(3, 3);
            this._baseRichTextBox.Size = new Size(this.Width - 6, this.Height - 6);

            this.Size = new Size(this._baseRichTextBox.Width + 6, this._baseRichTextBox.Height + 6);

            this._baseRichTextBox.TabStop = true;

            Controls.Add(this._baseRichTextBox);
        }

        /// <summary>
        /// Add event handler.
        /// </summary>
        private void AddEventHandler()
        {
            this._baseRichTextBox.AcceptsTabChanged += this.BaseRichTextBoxAcceptsTabChanged;
            this._baseRichTextBox.CausesValidationChanged += this.BaseRichTextBoxCausesValidationChanged;
            this._baseRichTextBox.ChangeUICues += this.BaseRichTextBoxChangeUICues;
            this._baseRichTextBox.Click += this.BaseRichTextBoxClick;
            this._baseRichTextBox.ClientSizeChanged += this.BaseRichTextBoxClientSizeChanged;
            this._baseRichTextBox.ContextMenuChanged += this.BaseRichTextBoxContextMenuChanged;
            this._baseRichTextBox.ContextMenuStripChanged += this.BaseRichTextBoxContextMenuStripChanged;
            this._baseRichTextBox.CursorChanged += this.BaseRichTextBoxCursorChanged;
            this._baseRichTextBox.KeyDown += this.BaseRichTextBoxKeyDown;
            this._baseRichTextBox.KeyPress += this.BaseRichTextBoxKeyPress;
            this._baseRichTextBox.KeyUp += this.BaseRichTextBoxKeyUp;
            this._baseRichTextBox.SizeChanged += this.BaseRichTextBoxSizeChanged;
            this._baseRichTextBox.TextChanged += this.BaseRichTextBoxTextChanged;
        }

        /// <summary>
        /// Update base text box.
        /// </summary>
        private void UpdateBaseTextBox()
        {
            if (this._baseRichTextBox == null)
            {
                return;
            }

            this._baseRichTextBox.Font = ModernFonts.TextBox(this._modernFontSize, this._modernFontWeight);
            this._baseRichTextBox.Location = new Point(3, 3);
            this._baseRichTextBox.Size = new Size(this.Width - 6, this.Height - 6);
        }

        /// <summary>
        /// Updates the Xml syntax highlight.
        /// </summary>
        /// <param name="force">true to force update; otherwise, false.</param>
        private void UpdateXmlSyntaxHighlight(bool force = false)
        {
            if (!this.IsValidXml(this.Text))
            {
                return;
            }

            if (!force && this._baseRichTextBox.Rtf.Equals((string)this._baseRichTextBox.Tag))
            {
                return;
            }

            lock (this._highlightSyncRoot)
            {
                if (this.IndentXml)
                {
                    this._baseRichTextBox.Text = this.ToIndentXml(this._baseRichTextBox.Text, this.OmitXmlDeclaration);
                }

                if (this.HighlightXmlSyntax)
                {
                    this._baseRichTextBox.Rtf = this.ToXmlSyntaxHighlightRtf(this._baseRichTextBox.Rtf, this.IndentXml, this.OmitXmlDeclaration, this.ThemeStyle == ModernThemeStyle.Dark);
                    this._baseRichTextBox.Tag = this._baseRichTextBox.Rtf;
                }
            }
        }

        /// <summary>
        /// Xml syntax highlight color.
        /// </summary>
        private static class XmlSyntaxHighlightColor
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

        /// <summary>
        /// PromptedRichTextBox user control.
        /// </summary>
        private class PromptedRichTextBox : RichTextBox
        {
            /// <summary>
            /// Field OCM_COMMAND.
            /// </summary>
            private const int OCM_COMMAND = 0x2111;

            /// <summary>
            /// Field WM_PAINT.
            /// </summary>
            private const int WM_PAINT = 15;

            /// <summary>
            /// Field _drawPrompt.
            /// </summary>
            private bool _drawPrompt;

            /// <summary>
            /// Field _promptText.
            /// </summary>
            private string _promptText = string.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="PromptedRichTextBox" /> class.
            /// </summary>
            public PromptedRichTextBox()
            {
                this._drawPrompt = this.Text == null || string.IsNullOrEmpty(this.Text.Trim());
            }

            /// <summary>
            /// Gets or sets prompt text.
            /// </summary>
            [Browsable(true)]
            [EditorBrowsable(EditorBrowsableState.Always)]
            [DefaultValue("")]
            public string PromptText
            {
                get
                {
                    return this._promptText;
                }

                set
                {
                    this._promptText = value.Trim();
                    this.Invalidate();
                }
            }

            /// <summary>
            /// OnPaint method.
            /// </summary>
            /// <param name="e">PaintEventArgs instance.</param>
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (this._drawPrompt)
                {
                    this.DrawTextPrompt(e.Graphics);
                }
            }

            /// <summary>
            /// OnTextChanged method.
            /// </summary>
            /// <param name="e">EventArgs instance.</param>
            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                this._drawPrompt = this.Text == null || string.IsNullOrEmpty(this.Text.Trim());
            }

            /// <summary>
            /// WndProc method.
            /// </summary>
            /// <param name="m">A Windows Message object.</param>
            protected override void WndProc(ref Message m)
            {
                if (((m.Msg == WM_PAINT) || (m.Msg == OCM_COMMAND)) && (this._drawPrompt && !this.GetStyle(ControlStyles.UserPaint)))
                {
                    this.DrawTextPrompt();
                }

                base.WndProc(ref m);
            }

            /// <summary>
            /// DrawTextPrompt method.
            /// </summary>
            private void DrawTextPrompt()
            {
                using (Graphics graphics = this.CreateGraphics())
                {
                    this.DrawTextPrompt(graphics);
                }
            }

            /// <summary>
            /// DrawTextPrompt method.
            /// </summary>
            /// <param name="g">Graphics instance.</param>
            private void DrawTextPrompt(Graphics g)
            {
                TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                Rectangle clientRectangle = ClientRectangle;
                clientRectangle.Offset(1, 1);
                TextRenderer.DrawText(g, this.PromptText, this.Font, clientRectangle, SystemColors.GrayText, this.BackColor, flags);
            }
        }
    }
}
