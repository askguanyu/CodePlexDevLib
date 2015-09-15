//-----------------------------------------------------------------------
// <copyright file="ByteParser.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    /// <summary>
    /// Byte parser.
    /// </summary>
    internal sealed class ByteParser
    {
        /// <summary>
        /// Field _bytes.
        /// </summary>
        private byte[] _bytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteParser"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal ByteParser(byte[] bytes)
        {
            this._bytes = bytes;
            this.CurrentOffset = 0;
        }

        /// <summary>
        /// Gets the current offset.
        /// </summary>
        internal int CurrentOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns>ByteString instance.</returns>
        internal ByteString ReadLine()
        {
            ByteString result = null;

            for (int i = this.CurrentOffset; i < this._bytes.Length; i++)
            {
                if (this._bytes[i] == 10)
                {
                    int num = i - this.CurrentOffset;

                    if (num > 0 && this._bytes[i - 1] == 13)
                    {
                        num--;
                    }

                    result = new ByteString(this._bytes, this.CurrentOffset, num);

                    this.CurrentOffset = i + 1;

                    return result;
                }
            }

            if (this.CurrentOffset < this._bytes.Length)
            {
                result = new ByteString(this._bytes, this.CurrentOffset, this._bytes.Length - this.CurrentOffset);
            }

            this.CurrentOffset = this._bytes.Length;

            return result;
        }
    }
}
