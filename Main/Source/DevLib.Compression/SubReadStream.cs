//-----------------------------------------------------------------------
// <copyright file="SubReadStream.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.IO;

    internal class SubReadStream : Stream
    {
        private readonly long _startInSuperStream;
        private readonly long _endInSuperStream;
        private readonly Stream _superStream;
        private long _positionInSuperStream;
        private bool _canRead;
        private bool _isDisposed;

        public SubReadStream(Stream superStream, long startPosition, long maxLength)
        {
            this._startInSuperStream = startPosition;
            this._positionInSuperStream = startPosition;
            this._endInSuperStream = startPosition + maxLength;
            this._superStream = superStream;
            this._canRead = true;
            this._isDisposed = false;
        }

        public override long Length
        {
            get
            {
                this.ThrowIfDisposed();
                return this._endInSuperStream - this._startInSuperStream;
            }
        }

        public override long Position
        {
            get
            {
                this.ThrowIfDisposed();
                return this._positionInSuperStream - this._startInSuperStream;
            }

            set
            {
                this.ThrowIfDisposed();
                throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
            }
        }

        public override bool CanRead
        {
            get
            {
                if (this._superStream.CanRead)
                {
                    return this._canRead;
                }

                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            this.ThrowIfCantRead();

            if (this._superStream.Position != this._positionInSuperStream)
            {
                this._superStream.Seek(this._positionInSuperStream, SeekOrigin.Begin);
            }

            if (this._positionInSuperStream + (long)count > this._endInSuperStream)
            {
                count = (int)(this._endInSuperStream - this._positionInSuperStream);
            }

            int num = this._superStream.Read(buffer, offset, count);
            this._positionInSuperStream += (long)num;

            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.ThrowIfDisposed();
            throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
        }

        public override void SetLength(long value)
        {
            this.ThrowIfDisposed();
            throw new NotSupportedException(CompressionConstants.SetLengthRequiresSeekingAndWriting);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            throw new NotSupportedException(CompressionConstants.WritingNotSupported);
        }

        public override void Flush()
        {
            this.ThrowIfDisposed();
            throw new NotSupportedException(CompressionConstants.WritingNotSupported);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this._isDisposed)
            {
                this._canRead = false;
                this._isDisposed = true;
            }

            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name, CompressionConstants.HiddenStreamName);
            }
        }

        private void ThrowIfCantRead()
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException(CompressionConstants.ReadingNotSupported);
            }
        }
    }
}
