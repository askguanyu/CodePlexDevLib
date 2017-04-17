//-----------------------------------------------------------------------
// <copyright file="BitStream.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.IO;

    /// <summary>
    /// Utility that read and write bits in byte array.
    /// </summary>
    internal class BitStream : Stream
    {
        /// <summary>
        /// The source bytes.
        /// </summary>
        private readonly byte[] _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitStream" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public BitStream(int capacity)
        {
            this._source = new byte[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitStream"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public BitStream(byte[] source)
        {
            this._source = source;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <value>The length.</value>
        public override long Length
        {
            get
            {
                return this._source.Length * 8;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value>The position.</value>
        public override long Position
        {
            get;
            set;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            this.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            long tempPos = this.Position;
            tempPos += offset;

            int readPosCount = 0, readPosMod = 0;

            long posCount = tempPos >> 3;
            int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < this.Position + offset + count && tempPos < this.Length)
            {
                if ((((int)this._source[posCount]) & (0x1 << (7 - posMod))) != 0)
                {
                    buffer[readPosCount] = (byte)((int)(buffer[readPosCount]) | (0x1 << (7 - readPosMod)));
                }
                else
                {
                    buffer[readPosCount] = (byte)((int)(buffer[readPosCount]) & (0xffffffff - (0x1 << (7 - readPosMod))));
                }

                tempPos++;

                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }

                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }

            int bits = (int)(tempPos - this.Position - offset);
            this.Position = tempPos;

            return bits;
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case (SeekOrigin.Begin):
                    this.Position = offset;
                    break;
                case (SeekOrigin.Current):
                    this.Position += offset;
                    break;
                case (SeekOrigin.End):
                    this.Position = this.Length + offset;
                    break;
            }

            return this.Position;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            long tempPos = this.Position;

            int readPosCount = offset >> 3, readPosMod = offset - ((offset >> 3) << 3);

            long posCount = tempPos >> 3;
            int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < this.Position + count && tempPos < this.Length)
            {
                if ((((int)buffer[readPosCount]) & (0x1 << (7 - readPosMod))) != 0)
                {
                    this._source[posCount] = (byte)((int)(this._source[posCount]) | (0x1 << (7 - posMod)));
                }
                else
                {
                    this._source[posCount] = (byte)((int)(this._source[posCount]) & (0xffffffff - (0x1 << (7 - posMod))));
                }

                tempPos++;

                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }

                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }

            this.Position = tempPos;
        }
    }
}
