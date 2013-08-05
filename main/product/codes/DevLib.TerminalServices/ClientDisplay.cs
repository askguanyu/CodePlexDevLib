//-----------------------------------------------------------------------
// <copyright file="ClientDisplay.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Information about a remote client's display.
    /// </summary>
    public class ClientDisplay
    {
        /// <summary>
        /// Field _bitsPerPixel.
        /// </summary>
        private readonly int _bitsPerPixel;

        /// <summary>
        /// Field _horizontalResolution.
        /// </summary>
        private readonly int _horizontalResolution;

        /// <summary>
        /// Field _verticalResolution.
        /// </summary>
        private readonly int _verticalResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDisplay" /> class.
        /// </summary>
        /// <param name="clientDisplay">WTS_CLIENT_DISPLAY instance.</param>
        internal ClientDisplay(WTS_CLIENT_DISPLAY clientDisplay)
        {
            this._horizontalResolution = clientDisplay.HorizontalResolution;
            this._verticalResolution = clientDisplay.VerticalResolution;
            this._bitsPerPixel = GetBitsPerPixel(clientDisplay.ColorDepth);
        }

        /// <summary>
        /// Gets the number of bits used per pixel in the client's connection to the session.
        /// </summary>
        public int BitsPerPixel
        {
            get
            {
                return this._bitsPerPixel;
            }
        }

        /// <summary>
        /// Gets the horizontal resolution of the client's display.
        /// </summary>
        /// <remarks>
        /// This may not be the same as the horizontal resolution of the client's monitor. It only reflects the size of the RDP connection window on the client.
        /// </remarks>
        public int HorizontalResolution
        {
            get
            {
                return this._horizontalResolution;
            }
        }

        /// <summary>
        /// Gets the vertical resolution of the client's display.
        /// </summary>
        /// <remarks>
        /// This may not be the same as the vertical resolution of the client's monitor. It only reflects the size of the RDP connection window on the client.
        /// </remarks>
        public int VerticalResolution
        {
            get
            {
                return this._verticalResolution;
            }
        }

        /// <summary>
        /// Method GetBitsPerPixel.
        /// </summary>
        /// <param name="colorDepth">Color depth.</param>
        /// <returns>The number of bits used per pixel</returns>
        private static int GetBitsPerPixel(int colorDepth)
        {
            switch (colorDepth)
            {
                case 1:
                    return 4;
                case 2:
                    return 8;
                case 4:
                    return 16;
                case 8:
                    return 24;
                case 16:
                    return 15;
            }

            return 0;
        }
    }
}