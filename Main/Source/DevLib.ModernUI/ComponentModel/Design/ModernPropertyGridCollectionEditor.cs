//-----------------------------------------------------------------------
// <copyright file="ModernPropertyGridCollectionEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// Provides a user interface that can edit most types of collections at design time. Collection changing can raise PropertyValueChanged.
    /// </summary>
    internal class ModernPropertyGridCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private static ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private static ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernPropertyGridCollectionEditor" /> class using the specified collection type.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public ModernPropertyGridCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public static event EventHandler CollectionPropertyValueChanged;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        public static ModernColorStyle ColorStyle
        {
            get
            {
                if (_modernColorStyle != ModernColorStyle.Default)
                {
                    return _modernColorStyle;
                }

                if (StyleManager != null && _modernColorStyle == ModernColorStyle.Default)
                {
                    return StyleManager.ColorStyle;
                }

                if (StyleManager == null && _modernColorStyle == ModernColorStyle.Default)
                {
                    return ModernConstants.DefaultColorStyle;
                }

                return _modernColorStyle;
            }

            set
            {
                _modernColorStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        public static ModernThemeStyle ThemeStyle
        {
            get
            {
                if (_modernThemeStyle != ModernThemeStyle.Default)
                {
                    return _modernThemeStyle;
                }

                if (StyleManager != null && _modernThemeStyle == ModernThemeStyle.Default)
                {
                    return StyleManager.ThemeStyle;
                }

                if (StyleManager == null && _modernThemeStyle == ModernThemeStyle.Default)
                {
                    return ModernConstants.DefaultThemeStyle;
                }

                return _modernThemeStyle;
            }

            set
            {
                _modernThemeStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        public static ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        public static bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Appends the custom attributes to target type.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public static void AppendCustomAttributes(Type type)
        {
            if (CanConvert(type))
            {
                return;
            }

            PropertyCustomTypeDescriptor propertyOverridingTypeDescriptor = new PropertyCustomTypeDescriptor(TypeDescriptor.GetProvider(GetEnumerableElementType(type)).GetTypeDescriptor(GetEnumerableElementType(type)));

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(type))
            {
                if (CanConvert(pd.PropertyType))
                {
                    continue;
                }

                PropertyDescriptor pdNew = null;

                if (pd.PropertyType.GetInterface("IEnumerable") != null)
                {
                    pdNew = TypeDescriptor.CreateProperty(
                        type,
                        pd,
                        new EditorAttribute(typeof(ModernPropertyGridCollectionEditor), typeof(UITypeEditor)),
                        ReadOnlyAttribute.No);
                }
                else
                {
                    pdNew = TypeDescriptor.CreateProperty(
                        type,
                        pd,
                        new TypeConverterAttribute(typeof(ExpandableObjectConverter<>).MakeGenericType(pd.PropertyType)),
                        ReadOnlyAttribute.No);
                }

                propertyOverridingTypeDescriptor.OverrideProperty(pdNew);
            }

            TypeDescriptor.AddProvider(new CustomTypeDescriptorProvider(propertyOverridingTypeDescriptor), type);
        }

        /// <summary>
        /// Edits the value of the specified object using the specified service provider and context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services can be obtained.</param>
        /// <param name="value">The object to edit the value of.</param>
        /// <returns>The new value of the object.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object result = base.EditValue(context, provider, value);

            if (CollectionPropertyValueChanged != null)
            {
                CollectionPropertyValueChanged(this, null);
            }

            return result;
        }

        /// <summary>
        /// Returns whether this converter can convert the object to a <see cref="T:System.String" /> and vice versa.
        /// </summary>
        /// <param name="type">A <see cref="T:System.Type" /> that represents the type you want to convert.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        internal static bool CanConvert(Type type)
        {
            if (Type.GetTypeCode(type) == TypeCode.Object
                && !type.IsEnum
                && type != typeof(Guid)
                && type != typeof(TimeSpan)
                && type != typeof(DateTimeOffset)
                && !IsNullableCanConvert(type))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new form to display and edit the current collection.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.Design.CollectionEditor.CollectionForm" /> to provide as the user interface for editing the collection.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm result = base.CreateCollectionForm();

            result.BackColor = ModernPaint.BackColor.Form(ThemeStyle);
            result.ForeColor = ModernPaint.ForeColor.Button.Normal(ThemeStyle);
            result.HelpButton = false;
            result.Size = new Size(640, 480);
            result.StartPosition = FormStartPosition.CenterParent;
            result.AcceptButton = null;

            foreach (Control item in result.Controls)
            {
                this.ConfigControl(item);
            }

            return result;
        }

        /// <summary>
        /// Creates a new instance of the specified collection item type.
        /// </summary>
        /// <param name="itemType">The type of item to create.</param>
        /// <returns>A new instance of the specified object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(string))
            {
                return string.Empty;
            }

            try
            {
                return base.CreateInstance(itemType);
            }
            catch
            {
                object result = null;

                try
                {
                    result = Activator.CreateInstance(itemType);
                }
                catch
                {
                    result = FormatterServices.GetUninitializedObject(itemType);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the data type that this collection contains.
        /// </summary>
        /// <returns>The data type of the items in the collection, or an <see cref="T:System.Object" /> if no Item property can be located on the collection.</returns>
        protected override Type CreateCollectionItemType()
        {
            Type result = base.CreateCollectionItemType();

            AppendCustomAttributes(result);

            return result;
        }

        /// <summary>
        /// Method IsNullableCanConvert.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the type is Nullable{} type; otherwise, false.</returns>
        private static bool IsNullableCanConvert(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && CanConvert(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Gets the element Type of the specified type which inherit IEnumerable interface.
        /// </summary>
        /// <param name="type">Source Type which inherit IEnumerable interface.</param>
        /// <returns>The System.Type of the element in the source list.</returns>
        private static Type GetEnumerableElementType(Type type)
        {
            if (type.GetInterface("IEnumerable") != null)
            {
                return type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            }

            return type;
        }

        /// <summary>
        /// Configures the control.
        /// </summary>
        /// <param name="control">The control.</param>
        private void ConfigControl(Control control)
        {
            control.BackColor = ModernPaint.BackColor.Form(ThemeStyle);
            control.ForeColor = ModernPaint.ForeColor.Button.Normal(ThemeStyle);

            if (control is Button)
            {
                Button button = (Button)control;

                button.Enter += (s, e) =>
                {
                    button.FlatAppearance.BorderSize = 0;
                };

                button.GotFocus += (s, e) =>
                {
                    button.BackColor = ModernPaint.BackColor.Button.Press(ThemeStyle);
                    button.ForeColor = ModernPaint.ForeColor.Button.Press(ThemeStyle);
                };

                button.Leave += (s, e) =>
                {
                    button.FlatAppearance.BorderSize = 1;
                };

                button.LostFocus += (s, e) =>
                {
                    button.BackColor = ModernPaint.BackColor.Button.Normal(ThemeStyle);
                    button.ForeColor = ModernPaint.ForeColor.Button.Normal(ThemeStyle);
                };

                button.EnabledChanged += (s, e) =>
                {
                    button.BackColor = button.Enabled ? ModernPaint.BackColor.Button.Normal(ThemeStyle) : ModernPaint.BackColor.Button.Disabled(ThemeStyle);
                    button.ForeColor = button.Enabled ? ModernPaint.ForeColor.Button.Normal(ThemeStyle) : ModernPaint.ForeColor.Button.Disabled(ThemeStyle);
                };

                button.Font = ModernFonts.Button(ModernFontSize.Small, ModernFontWeight.Bold);
                button.BackColor = button.Enabled ? ModernPaint.BackColor.Button.Normal(ThemeStyle) : ModernPaint.BackColor.Button.Disabled(ThemeStyle);
                button.ForeColor = button.Enabled ? ModernPaint.ForeColor.Button.Normal(ThemeStyle) : ModernPaint.ForeColor.Button.Disabled(ThemeStyle);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = button.Focused ? 0 : 1;
                button.FlatAppearance.BorderColor = ModernPaint.BorderColor.Button.Normal(ThemeStyle);
                button.FlatAppearance.MouseOverBackColor = ModernPaint.BackColor.Button.Hover(ThemeStyle);
                button.FlatAppearance.MouseDownBackColor = ModernPaint.BackColor.Button.Press(ThemeStyle);
                button.FlatAppearance.CheckedBackColor = ModernPaint.BorderColor.Button.Press(ThemeStyle);
                button.Height = 25;

                if (string.IsNullOrEmpty(button.Text) || string.IsNullOrEmpty(button.Text.Trim()))
                {
                    button.Width = 25;
                }
            }

            if (control is ListBox)
            {
                ListBox listBox = (ListBox)control;

                listBox.BorderStyle = BorderStyle.FixedSingle;

                listBox.DrawItem += (s, e) =>
                {
                    if (listBox.Items.Count > 0)
                    {
                        Color backColor = listBox.BackColor;
                        Color foreColor = listBox.ForeColor;

                        bool isSelected = (e.State & DrawItemState.Selected) != 0;
                        bool isHovered = (e.State & DrawItemState.HotLight) != 0;

                        if (isHovered)
                        {
                            backColor = ModernPaint.BackColor.Button.Hover(ThemeStyle);
                            foreColor = ModernPaint.ForeColor.Button.Hover(ThemeStyle);
                        }
                        else
                        {
                            if (isSelected)
                            {
                                backColor = listBox.Focused ? ControlPaint.Light(ModernPaint.GetStyleColor(ColorStyle), 0.2F) : ModernPaint.BackColor.Button.Disabled(ThemeStyle);
                                foreColor = listBox.Focused ? Color.FromArgb(17, 17, 17) : ModernPaint.ForeColor.Button.Normal(ThemeStyle);
                            }
                            else
                            {
                                backColor = ModernPaint.BackColor.Form(ThemeStyle);
                                foreColor = ModernPaint.ForeColor.Button.Normal(ThemeStyle);
                            }
                        }

                        e.DrawBackground();

                        using (SolidBrush brush = new SolidBrush(backColor))
                        {
                            e.Graphics.FillRectangle(brush, e.Bounds);
                        }

                        using (SolidBrush brush = new SolidBrush(foreColor))
                        {
                            e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, brush, listBox.GetItemRectangle(e.Index).Location);
                        }

                        e.DrawFocusRectangle();
                    }
                };
            }

            if (control is PropertyGrid)
            {
                PropertyGrid propertyGrid = (PropertyGrid)control;

                propertyGrid.CommandsBackColor = propertyGrid.BackColor;
                propertyGrid.HelpBackColor = propertyGrid.BackColor;
                propertyGrid.ViewBackColor = propertyGrid.BackColor;

                propertyGrid.ForeColor = propertyGrid.ForeColor;
                propertyGrid.CategoryForeColor = propertyGrid.ForeColor;
                propertyGrid.CommandsForeColor = propertyGrid.ForeColor;
                propertyGrid.HelpForeColor = propertyGrid.ForeColor;
                propertyGrid.ViewForeColor = propertyGrid.ForeColor;

                propertyGrid.CommandsLinkColor = ModernPaint.ForeColor.Link.Normal(ThemeStyle);
                propertyGrid.CommandsActiveLinkColor = ModernPaint.ForeColor.Link.Hover(ThemeStyle);
                propertyGrid.CommandsDisabledLinkColor = ModernPaint.ForeColor.Link.Disabled(ThemeStyle);
                propertyGrid.LineColor = UseStyleColors ? ControlPaint.Light(ModernPaint.GetStyleColor(ColorStyle), 0.2F) : ModernPaint.BackColor.Button.Normal(ThemeStyle);

                propertyGrid.HelpVisible = true;
                propertyGrid.PropertySort = PropertySort.NoSort;
                propertyGrid.ExpandAllGridItems();

                propertyGrid.Enter += (s, e) =>
                {
                    propertyGrid.LineColor = UseStyleColors ? ControlPaint.Light(ModernPaint.GetStyleColor(ColorStyle), 0.2F) : ModernPaint.BackColor.Button.Normal(ThemeStyle);
                };

                propertyGrid.Leave += (s, e) =>
                {
                    propertyGrid.LineColor = ModernPaint.BackColor.Button.Disabled(ThemeStyle);
                };
            }

            if (control.Controls.Count > 0)
            {
                foreach (Control item in control.Controls)
                {
                    this.ConfigControl(item);
                }
            }
        }
    }
}
