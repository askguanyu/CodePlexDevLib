//-----------------------------------------------------------------------
// <copyright file="ModernTabPageCollectionEditor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// ModernTabPage CollectionEditor.
    /// </summary>
    internal class ModernTabPageCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTabPageCollectionEditor"/> class.
        /// </summary>
        /// <param name="type">The type of the collection for this editor to edit.</param>
        public ModernTabPageCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Creates a new form to display and edit the current collection.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.Design.CollectionEditor.CollectionForm" /> to provide as the user interface for editing the collection.</returns>
        protected override CollectionForm CreateCollectionForm()
        {
            var baseForm = base.CreateCollectionForm();
            baseForm.Text = "ModernTabPage Collection Editor";
            return baseForm;
        }

        /// <summary>
        /// Gets the data type that this collection contains.
        /// </summary>
        /// <returns>The data type of the items in the collection, or an <see cref="T:System.Object" /> if no Item property can be located on the collection.</returns>
        protected override Type CreateCollectionItemType()
        {
            return typeof(ModernTabPage);
        }

        /// <summary>
        /// Gets the data types that this collection editor can contain.
        /// </summary>
        /// <returns>An array of data types that this collection can contain.</returns>
        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(ModernTabPage) };
        }
    }
}
