//-----------------------------------------------------------------------
// <copyright file="IModernForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using DevLib.ModernUI.ComponentModel;

    /// <summary>
    /// Interface of ModernForm.
    /// </summary>
    public interface IModernForm
    {
        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        ModernColorStyle ColorStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        ModernThemeStyle ThemeStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        ModernStyleManager StyleManager
        {
            get;
            set;
        }
    }
}