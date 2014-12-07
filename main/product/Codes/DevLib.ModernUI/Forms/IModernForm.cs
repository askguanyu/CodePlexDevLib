//-----------------------------------------------------------------------
// <copyright file="IModernForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System.ComponentModel;
    using DevLib.ModernUI.ComponentModel;

    /// <summary>
    /// Interface of ModernForm.
    /// </summary>
    public interface IModernForm
    {
        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        ModernColorStyle ColorStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        ModernThemeStyle ThemeStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom BackColor.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        bool UseCustomForeColor
        {
            get;
            set;
        }
    }
}