//-----------------------------------------------------------------------
// <copyright file="ModernPropertyGrid.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.ComponentModel.Design;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// Class PropertyGridUserControl.
    /// </summary>
    [ToolboxBitmap(typeof(PropertyGrid))]
    [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
    public class ModernPropertyGrid : PropertyGrid, IModernControl
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
        private ModernStyleManager _styleManager = null;

        /// <summary>
        /// Field _useStyleColors.
        /// </summary>
        private bool _useStyleColors = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernPropertyGrid" /> class.
        /// </summary>
        public ModernPropertyGrid()
        {
            base.PropertyValueChanged += this.OnPropertyValueChanged;
            ModernPropertyGridCollectionEditor.CollectionPropertyValueChanged += this.OnPropertyValueChanged;

            this.UseStyleColors = true;
            this.ApplyModernStyle();
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
        /// Occurs when a property value changes.
        /// </summary>
        public new event EventHandler PropertyValueChanged;

        /// <summary>
        /// Gets or sets the object for which the grid displays properties.
        /// </summary>
        public new object SelectedObject
        {
            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                if (base.SelectedObject == null)
                {
                    return null;
                }

                object selectedObject = null;

                try
                {
                    Type selectedObjectType = base.SelectedObject.GetType();

                    if (selectedObjectType.IsGenericType && selectedObjectType.GetGenericTypeDefinition().IsAssignableFrom(typeof(InnerConfig<>)))
                    {
                        selectedObject = typeof(InnerConfig<>).MakeGenericType(selectedObjectType.GetGenericArguments()[0]).GetProperty("Items").GetValue(base.SelectedObject, null);
                    }
                    else
                    {
                        selectedObject = base.SelectedObject;
                    }
                }
                catch
                {
                }

                return selectedObject;
            }

            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            set
            {
                if (value == null)
                {
                    base.SelectedObject = null;
                }
                else
                {
                    Type selectedObjectType = value.GetType();

                    if (selectedObjectType == typeof(string))
                    {
                        base.SelectedObject = value;
                    }
                    else if (selectedObjectType.GetInterface("IEnumerable") == null)
                    {
                        if (!ModernPropertyGridCollectionEditor.CanConvert(selectedObjectType))
                        {
                            ModernPropertyGridCollectionEditor.AppendCustomAttributes(selectedObjectType);
                        }

                        base.SelectedObject = value;
                    }
                    else
                    {
                        object innerConfig = Activator.CreateInstance(typeof(InnerConfig<>).MakeGenericType(selectedObjectType), value);
                        ModernPropertyGridCollectionEditor.AppendCustomAttributes(selectedObjectType);
                        ModernPropertyGridCollectionEditor.AppendCustomAttributes(innerConfig.GetType());
                        base.SelectedObject = innerConfig;
                    }
                }

                this.PropertySort = PropertySort.NoSort;
                this.ExpandAllGridItems();
                this.Refresh();
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
                ModernPropertyGridCollectionEditor.ColorStyle = value;
                this.ApplyModernStyle();
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
                ModernPropertyGridCollectionEditor.ThemeStyle = value;
                this.ApplyModernStyle();
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
                ModernPropertyGridCollectionEditor.StyleManager = value;
                this.ApplyModernStyle();
            }
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
            get
            {
                return this._useStyleColors;
            }

            set
            {
                this._useStyleColors = value;
                ModernPropertyGridCollectionEditor.UseStyleColors = value;
            }
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
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                Color backColor = this.BackColor;

                if (!this.UseCustomBackColor)
                {
                    backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                }

                if (backColor.A == 255 && this.BackgroundImage == null)
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
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
        /// Raises the <see cref="E:PaintForeground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
        }

        /// <summary>
        /// Applies the modern style.
        /// </summary>
        private void ApplyModernStyle()
        {
            this.BackColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            this.CommandsBackColor = this.BackColor;
            this.HelpBackColor = this.BackColor;
            this.ViewBackColor = this.BackColor;

            this.ForeColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);
            this.CategoryForeColor = this.ForeColor;
            this.CommandsForeColor = this.ForeColor;
            this.HelpForeColor = this.ForeColor;
            this.ViewForeColor = this.ForeColor;

            this.CommandsLinkColor = ModernPaint.ForeColor.Link.Normal(this.ThemeStyle);
            this.CommandsActiveLinkColor = ModernPaint.ForeColor.Link.Hover(this.ThemeStyle);
            this.CommandsDisabledLinkColor = ModernPaint.ForeColor.Link.Disabled(this.ThemeStyle);

            this.LineColor = this.UseStyleColors ? ModernPaint.GetStyleColor(this.ColorStyle) : ModernPaint.BackColor.Button.Normal(this.ThemeStyle);

            this.Refresh();
        }

        /// <summary>
        /// Method OnPropertyValueChanged.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnPropertyValueChanged(object sender, EventArgs e)
        {
            this.RaiseEvent(this.PropertyValueChanged);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Inner Class InnerConfig.
        /// </summary>
        /// <typeparam name="T">Type of configuration object.</typeparam>
        protected class InnerConfig<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InnerConfig{T}" /> class.
            /// </summary>
            /// <param name="configurationObject">Configuration object.</param>
            public InnerConfig(T configurationObject)
            {
                this.Items = configurationObject;
            }

            /// <summary>
            /// Gets or sets Items.
            /// </summary>
            public T Items
            {
                get;
                set;
            }
        }
    }
}
