//-----------------------------------------------------------------------
// <copyright file="PropertyCustomTypeDescriptor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Property CustomTypeDescriptor.
    /// </summary>
    public class PropertyCustomTypeDescriptor : CustomTypeDescriptor
    {
        /// <summary>
        /// Field _propertyDescriptorDictionary.
        /// </summary>
        private readonly Dictionary<string, PropertyDescriptor> _propertyDescriptorDictionary = new Dictionary<string, PropertyDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCustomTypeDescriptor"/> class.
        /// </summary>
        /// <param name="parent">The parent custom type descriptor.</param>
        public PropertyCustomTypeDescriptor(ICustomTypeDescriptor parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Returns an object that contains the property described by the specified property descriptor.
        /// </summary>
        /// <param name="pd">The property descriptor for which to retrieve the owning object.</param>
        /// <returns>An <see cref="T:System.Object" /> that owns the given property specified by the type descriptor. The default is null.</returns>
        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            object owner = base.GetPropertyOwner(pd);

            return owner ?? this;
        }

        /// <summary>
        /// Returns a collection of property descriptors for the object represented by this type descriptor.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> containing the property descriptions for the object represented by this type descriptor. The default is <see cref="F:System.ComponentModel.PropertyDescriptorCollection.Empty" />.</returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            return this.GetPropertiesImpl(base.GetProperties());
        }

        /// <summary>
        /// Returns a filtered collection of property descriptors for the object represented by this type descriptor.
        /// </summary>
        /// <param name="attributes">An array of attributes to use as a filter. This can be null.</param>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> containing the property descriptions for the object represented by this type descriptor. The default is <see cref="F:System.ComponentModel.PropertyDescriptorCollection.Empty" />.</returns>
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return this.GetPropertiesImpl(base.GetProperties(attributes));
        }

        /// <summary>
        /// Overrides the property.
        /// </summary>
        /// <param name="pd">The property descriptor.</param>
        public void OverrideProperty(PropertyDescriptor pd)
        {
            this._propertyDescriptorDictionary[pd.Name] = pd;
        }

        /// <summary>
        /// Gets the properties implementation.
        /// </summary>
        /// <param name="propertyDescriptorCollection">The property descriptor collection.</param>
        /// <returns>The new property descriptor collection.</returns>
        public PropertyDescriptorCollection GetPropertiesImpl(PropertyDescriptorCollection propertyDescriptorCollection)
        {
            List<PropertyDescriptor> pdList = new List<PropertyDescriptor>(propertyDescriptorCollection.Count + 1);

            foreach (PropertyDescriptor pd in propertyDescriptorCollection)
            {
                if (this._propertyDescriptorDictionary.ContainsKey(pd.Name))
                {
                    pdList.Add(this._propertyDescriptorDictionary[pd.Name]);
                }
                else
                {
                    pdList.Add(pd);
                }
            }

            PropertyDescriptorCollection result = new PropertyDescriptorCollection(pdList.ToArray());

            return result;
        }
    }
}
