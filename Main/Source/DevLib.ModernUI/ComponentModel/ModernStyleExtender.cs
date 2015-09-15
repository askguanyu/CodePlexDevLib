//-----------------------------------------------------------------------
// <copyright file="ModernStyleExtender.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// ModernStyleExtender class.
    /// </summary>
    [ProvideProperty("ApplyModernTheme", typeof(Control))]
    public sealed class ModernStyleExtender : Component, IExtenderProvider, IModernComponent
    {
        /// <summary>
        /// Field _extendedControls.
        /// </summary>
        private readonly List<Control> _extendedControls = new List<Control>();

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _modernStyleManager.
        /// </summary>
        private ModernStyleManager _modernStyleManager = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleExtender" /> class.
        /// </summary>
        public ModernStyleExtender()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernStyleExtender" /> class.
        /// </summary>
        /// <param name="parent">Parent container.</param>
        public ModernStyleExtender(IContainer parent)
        {
            if (parent != null)
            {
                parent.Add(this);
            }
        }

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernColorStyle ColorStyle
        {
            get;
            set;
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
                this.UpdateModernStyle();
            }
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernStyleManager StyleManager
        {
            get
            {
                return this._modernStyleManager;
            }

            set
            {
                this._modernStyleManager = value;
                this.UpdateModernStyle();
            }
        }

        /// <summary>
        /// Apply modern style.
        /// </summary>
        /// <param name="control">Control to apply.</param>
        /// <returns>true if apply succeeded; otherwise, false.</returns>
        public bool ApplyModernStyle(Control control)
        {
            return control != null && this._extendedControls.Contains(control);
        }

        /// <summary>
        /// Specifies whether this object can provide its extender properties to the specified object.
        /// </summary>
        /// <param name="extendee">The System.Object to receive the extender properties.</param>
        /// <returns>true if this object can provide extender properties to the specified object; otherwise, false.</returns>
        bool IExtenderProvider.CanExtend(object extendee)
        {
            return extendee is Control && !(extendee is IModernControl || extendee is IModernForm);
        }

        /// <summary>
        /// Update modern style.
        /// </summary>
        private void UpdateModernStyle()
        {
            Color backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            Color foreColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);

            foreach (Control item in this._extendedControls)
            {
                if (item != null)
                {
                    try
                    {
                        item.BackColor = backColor;
                    }
                    catch
                    {
                    }

                    try
                    {
                        item.ForeColor = foreColor;
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
