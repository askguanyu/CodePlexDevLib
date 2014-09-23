//-----------------------------------------------------------------------
// <copyright file="ProtocolStatistics.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Contains RDP protocol statistics.
    /// </summary>
    public class ProtocolStatistics
    {
        /// <summary>
        /// Field _bytes.
        /// </summary>
        private readonly int _bytes;

        /// <summary>
        /// Field _compressedBytes.
        /// </summary>
        private readonly int _compressedBytes;

        /// <summary>
        /// Field _frames.
        /// </summary>
        private readonly int _frames;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolStatistics" /> class.
        /// </summary>
        /// <param name="bytes">The number of bytes transferred.</param>
        /// <param name="frames">The number of frames transferred.</param>
        /// <param name="compressedBytes">The number of compressed bytes transferred.</param>
        public ProtocolStatistics(int bytes, int frames, int compressedBytes)
        {
            this._bytes = bytes;
            this._frames = frames;
            this._compressedBytes = compressedBytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolStatistics" /> class.
        /// </summary>
        /// <param name="counters">PROTOCOLCOUNTERS instance.</param>
        internal ProtocolStatistics(PROTOCOLCOUNTERS counters)
        {
            this._bytes = counters.Bytes;
            this._frames = counters.Frames;
            this._compressedBytes = counters.CompressedBytes;
        }

        /// <summary>
        /// Gets the number of bytes transferred.
        /// </summary>
        public int Bytes
        {
            get
            {
                return this._bytes;
            }
        }

        /// <summary>
        /// Gets the number of frames transferred.
        /// </summary>
        public int Frames
        {
            get
            {
                return this._frames;
            }
        }

        /// <summary>
        /// Gets the number of compressed bytes transferred.
        /// </summary>
        public int CompressedBytes
        {
            get
            {
                return this._compressedBytes;
            }
        }
    }
}