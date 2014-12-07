//-----------------------------------------------------------------------
// <copyright file="IModernControl.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// Interface of ModernControl.
    /// </summary>
    public interface IModernControl
    {
        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

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

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        bool UseSelectable
        {
            get;
            set;
        }
    }
}