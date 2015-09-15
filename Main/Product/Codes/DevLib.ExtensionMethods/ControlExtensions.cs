//-----------------------------------------------------------------------
// <copyright file="ControlExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Control Extensions.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Resizes the width of the columns.
        /// </summary>
        /// <param name="source">ListView object.</param>
        public static void AutoResizeColumns(this ListView source)
        {
            source.BeginUpdate();

            try
            {
                foreach (ColumnHeader column in source.Columns)
                {
                    column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

                    int columnContentWidth = column.Width;

                    column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

                    int headerWidth = column.Width;

                    column.Width = Math.Max(columnContentWidth, headerWidth);
                }
            }
            finally
            {
                source.EndUpdate();
            }
        }

        /// <summary>
        /// Highlights the Xml syntax for RichTextBox.
        /// </summary>
        /// <param name="source">The RichTextBox instance.</param>
        /// <param name="indentXml">true to indent Xml string; otherwise, keep the original string.</param>
        /// <param name="darkStyle">true to use dark style; otherwise, use light style.</param>
        public static void HighlightXmlSyntax(this RichTextBox source, bool indentXml = true, bool darkStyle = false)
        {
            if (!source.Text.IsValidXml())
            {
                return;
            }

            if (indentXml)
            {
                source.Text = source.Text.ToIndentXml();
            }

            source.Rtf = source.Rtf.ToXmlSyntaxHighlightRtf(indentXml);
        }
    }
}
