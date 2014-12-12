//-----------------------------------------------------------------------
// <copyright file="ModernToolTip.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernToolTip user control.
    /// </summary>
    [ToolboxBitmap(typeof(ToolTip))]
    public class ModernToolTip : ToolTip, IModernComponent
    {
        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Blue;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Light;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernToolTip" /> class.
        /// </summary>
        public ModernToolTip()
        {
            this.OwnerDraw = true;
            this.ShowAlways = true;

            this.Draw += new DrawToolTipEventHandler(this.ModernToolTipDraw);
            this.Popup += new PopupEventHandler(this.ModernToolTipPopup);
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
                if (this.StyleManager != null)
                {
                    return this.StyleManager.ColorStyle;
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
                if (this.StyleManager != null)
                {
                    return this.StyleManager.ThemeStyle;
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
        public ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Associates ToolTip text with the specified control.
        /// </summary>
        /// <param name="control">The control to associate the ToolTip text with.</param>
        /// <param name="caption">The ToolTip text to display when the pointer is on the control.</param>
        public new void SetToolTip(Control control, string caption)
        {
            base.SetToolTip(control, caption);

            if (control is IModernControl)
            {
                foreach (Control subControl in control.Controls)
                {
                    base.SetToolTip(subControl, caption);
                }
            }
        }

        /// <summary>
        /// ModernToolTipPopup method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        private void ModernToolTipPopup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedWindow is IModernForm)
            {
                this.ColorStyle = ((IModernForm)e.AssociatedWindow).ColorStyle;
                this.ThemeStyle = ((IModernForm)e.AssociatedWindow).ThemeStyle;
                this.StyleManager = ((IModernForm)e.AssociatedWindow).StyleManager;
            }
            else if (e.AssociatedControl is IModernControl)
            {
                this.ColorStyle = ((IModernControl)e.AssociatedControl).ColorStyle;
                this.ThemeStyle = ((IModernControl)e.AssociatedControl).ThemeStyle;
                this.StyleManager = ((IModernControl)e.AssociatedControl).StyleManager;
            }

            e.ToolTipSize = new Size(e.ToolTipSize.Width + 24, e.ToolTipSize.Height + 9);
        }

        /// <summary>
        /// ModernToolTipDraw method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        private void ModernToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            ModernThemeStyle displayTheme = this.ThemeStyle == ModernThemeStyle.Light ? ModernThemeStyle.Dark : ModernThemeStyle.Light;

            Color backColor = ModernPaint.BackColor.Form(displayTheme);
            Color borderColor = ModernPaint.BorderColor.Button.Normal(displayTheme);
            Color foreColor = ModernPaint.ForeColor.Label.Normal(displayTheme);

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (Pen pen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }

            Font font = ModernFonts.GetDefaultFont(13f, ModernFontWeight.Regular);

            TextRenderer.DrawText(e.Graphics, e.ToolTipText, font, e.Bounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}