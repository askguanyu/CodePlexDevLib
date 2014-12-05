//-----------------------------------------------------------------------
// <copyright file="ModernFonts.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System.Drawing;

    /// <summary>
    /// Font of modern style.
    /// </summary>
    public static class ModernFonts
    {
        /// <summary>
        /// Field FontResolver.
        /// </summary>
        private static readonly IModernFontResolver FontResolver = new DefaultFontResolver();

        /// <summary>
        /// Gets Font of Title.
        /// </summary>
        public static Font Title
        {
            get
            {
                return GetDefaultFont(24f, ModernFontWeight.Light);
            }
        }

        /// <summary>
        /// Gets Font of Subtitle.
        /// </summary>
        public static Font Subtitle
        {
            get
            {
                return GetDefaultFont(14f, ModernFontWeight.Regular);
            }
        }

        /// <summary>
        /// Gets Font of TileCount.
        /// </summary>
        public static Font TileCount
        {
            get
            {
                return GetDefaultFont(44f, ModernFontWeight.Regular);
            }
        }

        /// <summary>
        /// Gets Default Font.
        /// </summary>
        /// <param name="size">Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font GetDefaultFont(float size, ModernFontWeight weight)
        {
            switch (weight)
            {
                case ModernFontWeight.Light:
                    return FontResolver.ResolveFont("Segoe UI Light", size, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Regular:
                    return FontResolver.ResolveFont("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Bold:
                    return FontResolver.ResolveFont("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
                default:
                    return FontResolver.ResolveFont("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Gets Font of Tile.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font Tile(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of Link.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font Link(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of ComboBox.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font ComboBox(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of DateTime.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font DateTime(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of Label.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font Label(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of TextBox.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font TextBox(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of ProgressBar.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font ProgressBar(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of TabControl.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font TabControl(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of CheckBox.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font CheckBox(ModernFontSize size, ModernFontWeight weight)
        {
            return GetDefaultFont(size, weight);
        }

        /// <summary>
        /// Gets Font of Button.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        public static Font Button(ModernFontSize size, ModernFontWeight weight)
        {
            float fontSize = 11f;

            switch (size)
            {
                case ModernFontSize.Small:
                    fontSize = 11f;
                    break;
                case ModernFontSize.Medium:
                    fontSize = 13f;
                    break;
                case ModernFontSize.Large:
                    fontSize = 16f;
                    break;
                default:
                    break;
            }

            switch (weight)
            {
                case ModernFontWeight.Light:
                    return FontResolver.ResolveFont("Segoe UI Light", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Regular:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Bold:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                default:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Gets Default Font.
        /// </summary>
        /// <param name="size">Modern Font size.</param>
        /// <param name="weight">Modern Font weight.</param>
        /// <returns>Font instance.</returns>
        private static Font GetDefaultFont(ModernFontSize size, ModernFontWeight weight)
        {
            float fontSize = 14f;

            switch (size)
            {
                case ModernFontSize.Small:
                    fontSize = 12f;
                    break;
                case ModernFontSize.Medium:
                    fontSize = 14f;
                    break;
                case ModernFontSize.Large:
                    fontSize = 18f;
                    break;
                default:
                    break;
            }

            switch (weight)
            {
                case ModernFontWeight.Light:
                    return FontResolver.ResolveFont("Segoe UI Light", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Regular:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                case ModernFontWeight.Bold:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                default:
                    return FontResolver.ResolveFont("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            }
        }
    }
}