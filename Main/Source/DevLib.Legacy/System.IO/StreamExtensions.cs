#if NET35
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            return Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null);
        }

        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            return Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
        }

        public static void CopyTo(this Stream source, Stream destination)
        {
            CopyTo(source, destination, 16 * 1024);
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (!source.CanRead)
                throw new NotSupportedException("This stream does not support reading");
            if (!destination.CanWrite)
                throw new NotSupportedException("This destination stream does not support writing");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            var buffer = new byte[bufferSize];
            int nread;
            while ((nread = source.Read(buffer, 0, bufferSize)) != 0)
                destination.Write(buffer, 0, nread);
        }

        public static Task CopyToAsync(this Stream stream, Stream destination)
        {
#if NET45PLUS
            if (stream == null)
            throw new ArgumentNullException("stream");

            // This code requires the `Stream` class provide an implementation of `CopyToAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.CopyToAsync(destination);
#else
            return CopyToAsync(stream, destination, 16 * 1024, CancellationToken.None);
#endif
        }

        public static Task CopyToAsync(this Stream stream, Stream destination, int bufferSize)
        {
#if NET45PLUS
            if (stream == null)
            throw new ArgumentNullException("stream");

            // This code requires the `Stream` class provide an implementation of `CopyToAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.CopyToAsync(destination, bufferSize);
#else
            return CopyToAsync(stream, destination, bufferSize, CancellationToken.None);
#endif
        }

        public static Task CopyToAsync(this Stream stream, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (!stream.CanRead)
                throw new NotSupportedException("The stream does not support reading");
            if (!destination.CanWrite)
                throw new NotSupportedException("The destination does not support writing");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            if (cancellationToken.IsCancellationRequested)
                return GetCanceledTask<bool>();

#if NET45PLUS
            // This code requires the `Stream` class provide an implementation of `CopyToAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.CopyToAsync(destination, bufferSize, cancellationToken);
#else
            return CopyToAsync(stream, destination, new byte[bufferSize], cancellationToken);
#endif
        }

#if !NET45PLUS
#if UNITY
        private static Task CopyToAsync(Stream stream, Stream destination, byte[] buffer, CancellationToken cancellationToken)
        {
            int bytesRead;
            while ((bytesRead = ReadAsync(stream, buffer, 0, buffer.Length, cancellationToken).Await()) != 0)
            {
                destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).Await();
            }

            return Task.FromResult(true);
        }

#else

        private static async Task CopyToAsync(Stream stream, Stream destination, byte[] buffer, CancellationToken cancellationToken)
        {
            int bytesRead;
            while ((bytesRead = await ReadAsync(stream, buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

#endif
#endif

        public static Task FlushAsync(this Stream stream)
        {
#if NET45PLUS
            if (stream == null)
            throw new ArgumentNullException("stream");

            // This code requires the `Stream` class provide an implementation of `FlushAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.FlushAsync();
#else
            return FlushAsync(stream, CancellationToken.None);
#endif
        }

        public static Task FlushAsync(this Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (cancellationToken.IsCancellationRequested)
                return GetCanceledTask<bool>();

#if NET45PLUS
            // This code requires the `Stream` class provide an implementation of `FlushAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.FlushAsync(cancellationToken);
#else
            return Task.Factory.StartNew(state => ((Stream)state).Flush(), stream, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
#endif
        }

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
#if NET45PLUS
            if (stream == null)
            throw new ArgumentNullException("stream");

            // This code requires the `Stream` class provide an implementation of `FlushAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.ReadAsync(buffer, offset, count);
#else
            return ReadAsync(stream, buffer, offset, count, CancellationToken.None);
#endif
        }

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (cancellationToken.IsCancellationRequested)
                return GetCanceledTask<int>();

#if NET45PLUS
            // This code requires the `Stream` class provide an implementation of `ReadAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.ReadAsync(buffer, offset, count, cancellationToken);
#else
            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, TaskCreationOptions.None);
#endif
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
#if NET45PLUS
            if (stream == null)
            throw new ArgumentNullException("stream");

            // This code requires the `Stream` class provide an implementation of `WriteAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.WriteAsync(buffer, offset, count);
#else
            return WriteAsync(stream, buffer, offset, count, CancellationToken.None);
#endif
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (cancellationToken.IsCancellationRequested)
                return GetCanceledTask<bool>();

#if NET45PLUS
            // This code requires the `Stream` class provide an implementation of `WriteAsync`. The unit tests will
            // detect any case where this results in a stack overflow.
            return stream.WriteAsync(buffer, offset, count, cancellationToken);
#else
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, count, null);
#endif
        }

        private static Task<T> GetCanceledTask<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }
    }
}
#endif
