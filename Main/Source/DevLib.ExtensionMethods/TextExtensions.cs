//-----------------------------------------------------------------------
// <copyright file="TextExtensions.cs" company="Yu Guan Corporation">
//     Copyright (c) Yu Guan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Text;

    /// <summary>
    /// Text Extensions.
    /// </summary>
    public static class TextExtensions
    {
        /// <summary>
        /// Field CP1252Encoding.
        /// </summary>
        private static volatile Encoding CP1252Encoding;

        /// <summary>
        /// Gets an encoding for the CP1252 (Windows-1252) character set.
        /// </summary>
        public static Encoding CP1252
        {
            get
            {
                if (CP1252Encoding == null)
                {
                    CP1252Encoding = Encoding.GetEncoding(1252);
                }

                return CP1252Encoding;
            }
        }

        /// <summary>
        /// Gets an encoding for the CP1252 (Windows-1252) character set.
        /// </summary>
        /// <param name="source">Any object.</param>
        /// <returns>Encoding instance for the CP1252 (Windows-1252) character set.</returns>
        public static Encoding GetCP1252Encoding(this object source)
        {
            return Encoding.GetEncoding(1252);
        }
    }
}
