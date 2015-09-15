//-----------------------------------------------------------------------
// <copyright file="CustomTypeDescriptorProvider.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Custom TypeDescriptorProvider.
    /// </summary>
    public class CustomTypeDescriptorProvider : TypeDescriptionProvider
    {
        /// <summary>
        /// Field _customTypeDescriptor.
        /// </summary>
        private readonly ICustomTypeDescriptor _customTypeDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTypeDescriptorProvider" /> class.
        /// </summary>
        /// <param name="customTypeDescriptor">The custom type descriptor.</param>
        public CustomTypeDescriptorProvider(ICustomTypeDescriptor customTypeDescriptor)
        {
            this._customTypeDescriptor = customTypeDescriptor;
        }

        /// <summary>
        /// Gets a custom type descriptor for the given type and object.
        /// </summary>
        /// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
        /// <param name="instance">An instance of the type. Can be null if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor" />.</param>
        /// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for the type.</returns>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return this._customTypeDescriptor;
        }
    }
}
