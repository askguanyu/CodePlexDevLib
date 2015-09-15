//-----------------------------------------------------------------------
// <copyright file="StreamHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.IO;

    internal static class StreamHelper
    {
        public static void Copy(Stream source, Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            if (!source.CanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException((string)null, "Cannot access a closed Stream.");
            }

            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException("destination", "Cannot access a closed Stream.");
            }

            if (!source.CanRead)
            {
                throw new NotSupportedException("Stream does not support reading.");
            }

            if (!destination.CanWrite)
            {
                throw new NotSupportedException("Stream does not support writing.");
            }

            InternalCopy(source, destination, 81920);
        }

        private static void InternalCopy(Stream source, Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            int count;

            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }
    }
}
