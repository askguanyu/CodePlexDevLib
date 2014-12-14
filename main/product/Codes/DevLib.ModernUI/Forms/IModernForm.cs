//-----------------------------------------------------------------------
// <copyright file="IModernForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using DevLib.ModernUI.ComponentModel;

    /// <summary>
    /// Interface of ModernForm.
    /// </summary>
    public interface IModernForm
    {
        /// <summary>
        /// Event CloseBoxClick.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler CloseBoxClick;

        /// <summary>
        /// Event MinimizeBoxClick.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler MinimizeBoxClick;

        /// <summary>
        /// Event MaximizeBoxClick.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler MaximizeBoxClick;

        /// <summary>
        /// Event MaximizeBoxClick.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler NormalBoxClick;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernColorStyle.Default)]
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
        [DefaultValue(ModernThemeStyle.Default)]
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
        [DefaultValue(false)]
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
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        bool UseCustomForeColor
        {
            get;
            set;
        }
    }
}
