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
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

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
        /// Occurs when a property value changes.
        /// </summary>
        public static event EventHandler CollectionPropertyValueChanged;

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
        /// Creates a new form to display and edit the current collection.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.Design.CollectionEditor.CollectionForm" /> to provide as the user interface for editing the collection.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm result = base.CreateCollectionForm();

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
            }

            result.HelpButton = false;
            result.Size = new Size(640, 480);
            result.StartPosition = FormStartPosition.CenterParent;

            return result;
        }
    }
}
