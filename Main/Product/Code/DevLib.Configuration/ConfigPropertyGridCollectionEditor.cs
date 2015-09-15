//-----------------------------------------------------------------------
// <copyright file="ConfigPropertyGridCollectionEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Windows.Forms;

    /// <summary>
    /// Provides a user interface that can edit most types of collections at design time. Collection changing can raise PropertyValueChanged.
    /// </summary>
    public class ConfigPropertyGridCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigPropertyGridCollectionEditor" /> class using the specified collection type.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public ConfigPropertyGridCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public static event EventHandler CollectionPropertyValueChanged;

        /// <summary>
        /// Appends the custom attributes to target type.
        /// </summary>
        /// <param name="type">The type to add.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
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
                        new EditorAttribute(typeof(ConfigPropertyGridCollectionEditor), typeof(UITypeEditor)),
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
            if (Type.GetTypeCode(type) == TypeCode.Object &&
                !type.IsEnum &&
                type != typeof(Guid) &&
                type != typeof(TimeSpan) &&
                type != typeof(DateTimeOffset) &&
                !IsNullableCanConvert(type))
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

            result.HelpButton = false;
            result.Size = new Size(640, 480);
            result.StartPosition = FormStartPosition.CenterParent;

            PropertyGrid propertyGrid = null;

            try
            {
                propertyGrid = result.Controls[0].Controls["propertyBrowser"] as PropertyGrid;
            }
            catch
            {
            }

            if (propertyGrid != null)
            {
                propertyGrid.HelpVisible = true;
                propertyGrid.PropertySort = PropertySort.NoSort;
                propertyGrid.ExpandAllGridItems();
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
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
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
    }
}
