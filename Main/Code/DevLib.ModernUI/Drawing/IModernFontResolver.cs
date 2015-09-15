//-----------------------------------------------------------------------
// <copyright file="IModernFontResolver.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System.Drawing;

    /// <summary>
    /// Interface of ModernFontResolver.
    /// </summary>
    internal interface IModernFontResolver
    {
        /// <summary>
        /// Resolve a new System.Drawing.Font by using a specified size, style, and unit.
        /// </summary>
        /// <param name="familyName">A string representation of the System.Drawing.FontFamily for the new System.Drawing.Font.</param>
        /// <param name="emSize">The em-size of the new font in the units specified by the unit parameter.</param>
        /// <param name="style">The System.Drawing.FontStyle of the new font.</param>
        /// <param name="unit">The System.Drawing.GraphicsUnit of the new font.</param>
        /// <returns>Font instance.</returns>
        Font ResolveFont(string familyName, float emSize, FontStyle style, GraphicsUnit unit);
    }
}
