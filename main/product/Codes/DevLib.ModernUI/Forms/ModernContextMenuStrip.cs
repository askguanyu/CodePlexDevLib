//-----------------------------------------------------------------------
// <copyright file="ModernContextMenuStrip.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernContextMenuStrip user control.
    /// </summary>
    public class ModernContextMenuStrip : ContextMenuStrip, IModernControl
    {
        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _styleManager.
        /// </summary>
        private ModernStyleManager _styleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernContextMenuStrip"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ModernContextMenuStrip(IContainer container)
        {
            if (container != null)
            {
                container.Add(this);
            }
        }

        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Category("Modern Appearance")]
        public event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Category("Modern Appearance")]
        [DefaultValue(ModernColorStyle.Default)]
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
                    return ModernColorStyle.Blue;
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
        [Category("Modern Appearance")]
        [DefaultValue(ModernThemeStyle.Default)]
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
                    return ModernThemeStyle.Light;
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
            get
            {
                return this._styleManager;
            }

            set
            {
                this._styleManager = value;
                this.SetTheme();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom BackColor.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [DefaultValue(false)]
        [Category("Modern Appearance")]
        public bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(false)]
        [Category("Modern Behaviour")]
        [DefaultValue(false)]
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
        /// Sets the theme.
        /// </summary>
        private void SetTheme()
        {
            this.BackColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            this.ForeColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);
            this.Renderer = new ModernToolStripRenderer(this.ThemeStyle, this.ColorStyle);
        }

        /// <summary>
        /// ModernToolStrip renderer.
        /// </summary>
        private class ModernToolStripRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModernToolStripRenderer"/> class.
            /// </summary>
            /// <param name="themeStyle">Modern theme style.</param>
            /// <param name="colorStyle">Modern color style.</param>
            public ModernToolStripRenderer(ModernThemeStyle themeStyle, ModernColorStyle colorStyle)
                : base(new ContextColors(themeStyle, colorStyle))
            {
            }
        }

        /// <summary>
        /// ContextColors class.
        /// </summary>
        private class ContextColors : ProfessionalColorTable
        {
            /// <summary>
            /// Field _modernThemeStyle.
            /// </summary>
            private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Light;

            /// <summary>
            /// Field _modernColorStyle.
            /// </summary>
            private ModernColorStyle _modernColorStyle = ModernColorStyle.Blue;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextColors"/> class.
            /// </summary>
            /// <param name="themeStyle">Modern theme style.</param>
            /// <param name="colorStyle">Modern color style.</param>
            public ContextColors(ModernThemeStyle themeStyle, ModernColorStyle colorStyle)
            {
                this._modernThemeStyle = themeStyle;
                this._modernColorStyle = colorStyle;
            }

            /// <summary>
            /// Gets the solid color to use when a <see cref="T:System.Windows.Forms.ToolStripMenuItem" /> other than the top-level <see cref="T:System.Windows.Forms.ToolStripMenuItem" /> is selected.
            /// </summary>
            public override Color MenuItemSelected
            {
                get
                {
                    return ModernPaint.GetStyleColor(this._modernColorStyle);
                }
            }

            /// <summary>
            /// Gets the color that is the border color to use on a <see cref="T:System.Windows.Forms.MenuStrip" />.
            /// </summary>
            public override Color MenuBorder
            {
                get
                {
                    return ModernPaint.BackColor.Form(this._modernThemeStyle);
                }
            }

            /// <summary>
            /// Gets the border color to use with a <see cref="T:System.Windows.Forms.ToolStripMenuItem" />.
            /// </summary>
            public override Color MenuItemBorder
            {
                get
                {
                    return ModernPaint.GetStyleColor(this._modernColorStyle);
                }
            }

            /// <summary>
            /// Gets the starting color of the gradient used in the image margin of a <see cref="T:System.Windows.Forms.ToolStripDropDownMenu" />.
            /// </summary>
            public override Color ImageMarginGradientBegin
            {
                get
                {
                    return ModernPaint.BackColor.Form(this._modernThemeStyle);
                }
            }

            /// <summary>
            /// Gets the middle color of the gradient used in the image margin of a <see cref="T:System.Windows.Forms.ToolStripDropDownMenu" />.
            /// </summary>
            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return ModernPaint.BackColor.Form(this._modernThemeStyle);
                }
            }

            /// <summary>
            /// Gets the end color of the gradient used in the image margin of a <see cref="T:System.Windows.Forms.ToolStripDropDownMenu" />.
            /// </summary>
            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return ModernPaint.BackColor.Form(this._modernThemeStyle);
                }
            }
        }
    }
}