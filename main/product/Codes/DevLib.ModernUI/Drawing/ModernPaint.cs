//-----------------------------------------------------------------------
// <copyright file="ModernPaint.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.Forms;

    /// <summary>
    /// ModernPaint class.
    /// </summary>
    public static class ModernPaint
    {
        /// <summary>
        /// Get modern style color.
        /// </summary>
        /// <param name="colorStyle">Modern color style.</param>
        /// <returns>Color instance.</returns>
        public static Color GetStyleColor(ModernColorStyle colorStyle)
        {
            switch (colorStyle)
            {
                case ModernColorStyle.Black:
                    return ModernColors.Black;

                case ModernColorStyle.White:
                    return ModernColors.White;

                case ModernColorStyle.Silver:
                    return ModernColors.Silver;

                case ModernColorStyle.Blue:
                    return ModernColors.Blue;

                case ModernColorStyle.Green:
                    return ModernColors.Green;

                case ModernColorStyle.Lime:
                    return ModernColors.Lime;

                case ModernColorStyle.Teal:
                    return ModernColors.Teal;

                case ModernColorStyle.Orange:
                    return ModernColors.Orange;

                case ModernColorStyle.Brown:
                    return ModernColors.Brown;

                case ModernColorStyle.Pink:
                    return ModernColors.Pink;

                case ModernColorStyle.Magenta:
                    return ModernColors.Magenta;

                case ModernColorStyle.Purple:
                    return ModernColors.Purple;

                case ModernColorStyle.Red:
                    return ModernColors.Red;

                case ModernColorStyle.Yellow:
                    return ModernColors.Yellow;

                default:
                    return ModernColors.Blue;
            }
        }

        /// <summary>
        /// Get modern style brush.
        /// </summary>
        /// <param name="colorStyle">Modern color style.</param>
        /// <returns>SolidBrush instance.</returns>
        public static SolidBrush GetStyleBrush(ModernColorStyle colorStyle)
        {
            switch (colorStyle)
            {
                case ModernColorStyle.Black:
                    return ModernBrushes.Black;

                case ModernColorStyle.White:
                    return ModernBrushes.White;

                case ModernColorStyle.Silver:
                    return ModernBrushes.Silver;

                case ModernColorStyle.Blue:
                    return ModernBrushes.Blue;

                case ModernColorStyle.Green:
                    return ModernBrushes.Green;

                case ModernColorStyle.Lime:
                    return ModernBrushes.Lime;

                case ModernColorStyle.Teal:
                    return ModernBrushes.Teal;

                case ModernColorStyle.Orange:
                    return ModernBrushes.Orange;

                case ModernColorStyle.Brown:
                    return ModernBrushes.Brown;

                case ModernColorStyle.Pink:
                    return ModernBrushes.Pink;

                case ModernColorStyle.Magenta:
                    return ModernBrushes.Magenta;

                case ModernColorStyle.Purple:
                    return ModernBrushes.Purple;

                case ModernColorStyle.Red:
                    return ModernBrushes.Red;

                case ModernColorStyle.Yellow:
                    return ModernBrushes.Yellow;

                default:
                    return ModernBrushes.Blue;
            }
        }

        /// <summary>
        /// Get modern style pen.
        /// </summary>
        /// <param name="colorStyle">Modern color style.</param>
        /// <returns>Pen instance.</returns>
        public static Pen GetStylePen(ModernColorStyle colorStyle)
        {
            switch (colorStyle)
            {
                case ModernColorStyle.Black:
                    return ModernPens.Black;

                case ModernColorStyle.White:
                    return ModernPens.White;

                case ModernColorStyle.Silver:
                    return ModernPens.Silver;

                case ModernColorStyle.Blue:
                    return ModernPens.Blue;

                case ModernColorStyle.Green:
                    return ModernPens.Green;

                case ModernColorStyle.Lime:
                    return ModernPens.Lime;

                case ModernColorStyle.Teal:
                    return ModernPens.Teal;

                case ModernColorStyle.Orange:
                    return ModernPens.Orange;

                case ModernColorStyle.Brown:
                    return ModernPens.Brown;

                case ModernColorStyle.Pink:
                    return ModernPens.Pink;

                case ModernColorStyle.Magenta:
                    return ModernPens.Magenta;

                case ModernColorStyle.Purple:
                    return ModernPens.Purple;

                case ModernColorStyle.Red:
                    return ModernPens.Red;

                case ModernColorStyle.Yellow:
                    return ModernPens.Yellow;

                default:
                    return ModernPens.Blue;
            }
        }

        /// <summary>
        /// Get StringFormat.
        /// </summary>
        /// <param name="textAlign">Specifies alignment of text on the drawing surface.</param>
        /// <returns>StringFormat instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static StringFormat GetStringFormat(ContentAlignment textAlign)
        {
            StringFormat stringFormat = new StringFormat();
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;

            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                    stringFormat.Alignment = StringAlignment.Near;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;

                case ContentAlignment.TopCenter:
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;

                case ContentAlignment.TopRight:
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;

                case ContentAlignment.MiddleLeft:
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;

                case ContentAlignment.MiddleCenter:
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    break;

                case ContentAlignment.MiddleRight:
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Far;
                    break;

                case ContentAlignment.BottomLeft:
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    break;

                case ContentAlignment.BottomCenter:
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    break;

                case ContentAlignment.BottomRight:
                    stringFormat.Alignment = StringAlignment.Far;
                    stringFormat.LineAlignment = StringAlignment.Far;
                    break;
            }

            return stringFormat;
        }

        /// <summary>
        /// Get TextFormatFlags.
        /// </summary>
        /// <param name="textAlign">Specifies alignment of text on the drawing surface.</param>
        /// <returns>TextFormatFlags instance.</returns>
        public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign)
        {
            return GetTextFormatFlags(textAlign, false);
        }

        /// <summary>
        /// Get TextFormatFlags.
        /// </summary>
        /// <param name="textAlign">Specifies alignment of text on the drawing surface.</param>
        /// <param name="wrap">Wrap text or not.</param>
        /// <returns>TextFormatFlags instance.</returns>
        public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign, bool wrap)
        {
            TextFormatFlags controlFlags = wrap ? TextFormatFlags.WordBreak : TextFormatFlags.EndEllipsis;

            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.Left;
                    break;

                case ContentAlignment.TopCenter:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                    break;

                case ContentAlignment.TopRight:
                    controlFlags |= TextFormatFlags.Top | TextFormatFlags.Right;
                    break;

                case ContentAlignment.MiddleLeft:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                    break;

                case ContentAlignment.MiddleCenter:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    break;

                case ContentAlignment.MiddleRight:
                    controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                    break;

                case ContentAlignment.BottomLeft:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
                    break;

                case ContentAlignment.BottomCenter:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                    break;

                case ContentAlignment.BottomRight:
                    controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
                    break;
            }

            return controlFlags;
        }

        /// <summary>
        /// Get color by theme style.
        /// </summary>
        /// <param name="themeStyle">Modern theme style.</param>
        /// <param name="darkRGB">RGB value for the Dark style.</param>
        /// <param name="restRGB">RGB value for the rest style.</param>
        /// <returns>Color instance.</returns>
        private static Color GetColorByThemeStyle(ModernThemeStyle themeStyle, int darkRGB, int restRGB)
        {
            if (themeStyle == ModernThemeStyle.Dark)
            {
                return Color.FromArgb(darkRGB, darkRGB, darkRGB);
            }

            return Color.FromArgb(restRGB, restRGB, restRGB);
        }

        /// <summary>
        /// BorderColor class.
        /// </summary>
        public static class BorderColor
        {
            /// <summary>
            /// Get form color.
            /// </summary>
            /// <param name="themeStyle">Modern theme style.</param>
            /// <returns>Color instance.</returns>
            public static Color Form(ModernThemeStyle themeStyle)
            {
                return GetColorByThemeStyle(themeStyle, 68, 204);
            }

            /// <summary>
            /// Button class.
            /// </summary>
            public static class Button
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 102);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 238, 51);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 109, 155);
                }
            }

            /// <summary>
            /// CheckBox class.
            /// </summary>
            public static class CheckBox
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 204, 51);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 85, 204);
                }
            }

            /// <summary>
            /// ComboBox class.
            /// </summary>
            public static class ComboBox
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 204, 51);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 85, 204);
                }
            }

            /// <summary>
            /// ProgressBar class.
            /// </summary>
            public static class ProgressBar
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 109, 155);
                }
            }

            /// <summary>
            /// TabControl class.
            /// </summary>
            public static class TabControl
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 68, 204);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 109, 155);
                }
            }
        }

        /// <summary>
        /// BackColor class.
        /// </summary>
        public static class BackColor
        {
            /// <summary>
            /// Get form color.
            /// </summary>
            /// <param name="themeStyle">Modern theme style.</param>
            /// <returns>Color instance.</returns>
            public static Color Form(ModernThemeStyle themeStyle)
            {
                return GetColorByThemeStyle(themeStyle, 17, 255);
            }

            /// <summary>
            /// Button class.
            /// </summary>
            public static class Button
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 34, 238);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 102);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 238, 51);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 80, 204);
                }
            }

            /// <summary>
            /// TrackBar class.
            /// </summary>
            public static class TrackBar
            {
                /// <summary>
                /// Thumb class.
                /// </summary>
                public static class Thumb
                {
                    /// <summary>
                    /// Get normal color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Normal(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 153, 102);
                    }

                    /// <summary>
                    /// Get hover color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Hover(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 204, 17);
                    }

                    /// <summary>
                    /// Get press color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Press(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 204, 17);
                    }

                    /// <summary>
                    /// Get disabled color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Disabled(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 85, 179);
                    }
                }

                /// <summary>
                /// Bar class.
                /// </summary>
                public static class Bar
                {
                    /// <summary>
                    /// Get normal color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Normal(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 204);
                    }

                    /// <summary>
                    /// Get hover color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Hover(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 204);
                    }

                    /// <summary>
                    /// Get press color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Press(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 204);
                    }

                    /// <summary>
                    /// Get disabled color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Disabled(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 34, 230);
                    }
                }
            }

            /// <summary>
            /// ScrollBar class.
            /// </summary>
            public static class ScrollBar
            {
                /// <summary>
                /// Thumb class.
                /// </summary>
                public static class Thumb
                {
                    /// <summary>
                    /// Get normal color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Normal(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 221);
                    }

                    /// <summary>
                    /// Get hover color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Hover(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 204, 17);
                    }

                    /// <summary>
                    /// Get press color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Press(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 204, 17);
                    }

                    /// <summary>
                    /// Get disabled color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Disabled(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 221);
                    }
                }

                /// <summary>
                /// Bar class.
                /// </summary>
                public static class Bar
                {
                    /// <summary>
                    /// Get normal color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Normal(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get hover color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Hover(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get press color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Press(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get disabled color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Disabled(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }
                }
            }

            /// <summary>
            /// ProgressBar class.
            /// </summary>
            public static class ProgressBar
            {
                /// <summary>
                /// Bar class.
                /// </summary>
                public static class Bar
                {
                    /// <summary>
                    /// Get normal color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Normal(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get hover color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Hover(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get press color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Press(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 38, 234);
                    }

                    /// <summary>
                    /// Get disabled color.
                    /// </summary>
                    /// <param name="themeStyle">Modern theme style.</param>
                    /// <returns>Color instance.</returns>
                    public static Color Disabled(ModernThemeStyle themeStyle)
                    {
                        return GetColorByThemeStyle(themeStyle, 51, 221);
                    }
                }
            }
        }

        /// <summary>
        /// ForeColor class.
        /// </summary>
        public static class ForeColor
        {
            /// <summary>
            /// Get title color.
            /// </summary>
            /// <param name="themeStyle">Modern theme style.</param>
            /// <returns>Color instance.</returns>
            public static Color Title(ModernThemeStyle themeStyle)
            {
                return GetColorByThemeStyle(themeStyle, 255, 0);
            }

            /// <summary>
            /// Button class.
            /// </summary>
            public static class Button
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 204, 0);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 17, 255);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 17, 255);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 109, 136);
                }
            }

            /// <summary>
            /// Tile class.
            /// </summary>
            public static class Tile
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 255, 255);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 255, 255);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 255, 255);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 209, 209);
                }
            }

            /// <summary>
            /// Link class.
            /// </summary>
            public static class Link
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 0);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 93, 128);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 93, 128);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 51, 209);
                }
            }

            /// <summary>
            /// Label class.
            /// </summary>
            public static class Label
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 0);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 51, 209);
                }
            }

            /// <summary>
            /// CheckBox class.
            /// </summary>
            public static class CheckBox
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 17);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 93, 136);
                }
            }

            /// <summary>
            /// ComboBox class.
            /// </summary>
            public static class ComboBox
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get hover color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Hover(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 17);
                }

                /// <summary>
                /// Get press color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Press(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 153, 153);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 93, 136);
                }
            }

            /// <summary>
            /// ProgressBar class.
            /// </summary>
            public static class ProgressBar
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 0);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 51, 209);
                }
            }

            /// <summary>
            /// TabControl class.
            /// </summary>
            public static class TabControl
            {
                /// <summary>
                /// Get normal color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Normal(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 170, 0);
                }

                /// <summary>
                /// Get disabled color.
                /// </summary>
                /// <param name="themeStyle">Modern theme style.</param>
                /// <returns>Color instance.</returns>
                public static Color Disabled(ModernThemeStyle themeStyle)
                {
                    return GetColorByThemeStyle(themeStyle, 51, 209);
                }
            }
        }
    }
}