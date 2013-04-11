//-----------------------------------------------------------------------
// <copyright file="PropertyValueChangedCollectionEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Provides a user interface that can edit most types of collections at design time. Collection changing can raise PropertyValueChanged.
    /// </summary>
    public class PropertyValueChangedCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueChangedCollectionEditor" /> class using the specified collection type.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public PropertyValueChangedCollectionEditor(Type type)
            : base(type)
        {
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
            object source = base.EditValue(context, provider, value);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
