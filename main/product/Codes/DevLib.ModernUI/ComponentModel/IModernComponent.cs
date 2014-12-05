//-----------------------------------------------------------------------
// <copyright file="IModernComponent.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.ComponentModel
{
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// Interface of ModernComponent.
    /// </summary>
    public interface IModernComponent
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