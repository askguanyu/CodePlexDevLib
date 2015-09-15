//-----------------------------------------------------------------------
// <copyright file="ByteString.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// ByteString class.
    /// </summary>
    internal sealed class ByteString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteString"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public ByteString(byte[] bytes, int offset, int length)
        {
            this.Bytes = bytes;

            if (this.Bytes != null && offset >= 0 && length >= 0 && offset + length <= this.Bytes.Length)
            {
                this.Offset = offset;
                this.Length = length;
            }
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        public byte[] Bytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.Bytes == null || this.Length == 0;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Byte of the index.</returns>
        public byte this[int index]
        {
            get
            {
                return this.Bytes[this.Offset + index];
            }
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>The string.</returns>
        public string GetString()
        {
            return this.GetString(Encoding.UTF8);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The string.</returns>
        public string GetString(Encoding encoding)
        {
            if (this.IsEmpty)
            {
                return string.Empty;
            }

            return encoding.GetString(this.Bytes, this.Offset, this.Length);
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>Byte array.</returns>
        public byte[] GetBytes()
        {
            byte[] result = new byte[this.Length];

            if (this.Length > 0)
            {
                Buffer.BlockCopy(this.Bytes, this.Offset, result, 0, this.Length);
            }

            return result;
        }

        /// <summary>
        /// Gets index of the char.
        /// </summary>
        /// <param name="value">The char.</param>
        /// <returns>Index of the char.</returns>
        public int IndexOf(char value)
        {
            return this.IndexOf(value, 0);
        }

        /// <summary>
        /// Gets index of the char.
        /// </summary>
        /// <param name="value">The char.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>Index of the char.</returns>
        public int IndexOf(char value, int offset)
        {
            for (int i = offset; i < this.Length; i++)
            {
                if (this[i] == (byte)value)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets substring of the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>ByteString instance.</returns>
        public ByteString Substring(int offset)
        {
            return this.Substring(offset, this.Length - offset);
        }

        /// <summary>
        /// Gets substring the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>ByteString instance.</returns>
        public ByteString Substring(int offset, int length)
        {
            return new ByteString(this.Bytes, this.Offset + offset, length);
        }

        /// <summary>
        /// Returns ByteString array that contains the substrings in this instance that are delimited by elements of a specified Unicode character.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>ByteString array.</returns>
        public List<ByteString> Split(char separator)
        {
            List<ByteString> result = new List<ByteString>();

            int i = 0;

            while (i < this.Length)
            {
                int num = this.IndexOf(separator, i);

                if (num < 0)
                {
                    result.Add(this.Substring(i));
                    break;
                }

                result.Add(this.Substring(i, num - i));

                i = num + 1;

                while (i < this.Length && this[i] == (byte)separator)
                {
                    i++;
                }
            }

            return result;
        }
    }
}
