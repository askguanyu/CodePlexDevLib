//-----------------------------------------------------------------------
// <copyright file="CheckSumAndSizeWriteStream.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.IO;

    internal class CheckSumAndSizeWriteStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly Stream _baseBaseStream;
        private readonly bool _leaveOpenOnClose;
        private readonly ActionHelper<long, long, uint> _saveCrcAndSizes;
        private long _position;
        private uint _checksum;
        private bool _canWrite;
        private bool _isDisposed;
        private bool _everWritten;
        private long _initialPosition;

        public CheckSumAndSizeWriteStream(Stream baseStream, Stream baseBaseStream, bool leaveOpenOnClose, ActionHelper<long, long, uint> saveCrcAndSizes)
        {
            this._baseStream = baseStream;
            this._baseBaseStream = baseBaseStream;
            this._position = 0L;
            this._checksum = 0U;
            this._leaveOpenOnClose = leaveOpenOnClose;
            this._canWrite = true;
            this._isDisposed = false;
            this._initialPosition = 0L;
            this._saveCrcAndSizes = saveCrcAndSizes;
        }

        public override long Length
        {
            get
            {
                this.ThrowIfDisposed();
                throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
            }
        }

        public override long Position
        {
            get
            {
                this.ThrowIfDisposed();
                return this._position;
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
                return this._canWrite;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            throw new NotSupportedException(CompressionConstants.ReadingNotSupported);
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
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", CompressionConstants.ArgumentNeedNonNegative);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", CompressionConstants.ArgumentNeedNonNegative);
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException(CompressionConstants.OffsetLengthInvalid);
            }

            this.ThrowIfDisposed();

            if (count == 0)
            {
                return;
            }

            if (!this._everWritten)
            {
                this._initialPosition = this._baseBaseStream.Position;
                this._everWritten = true;
            }

            this._checksum = Crc32Helper.UpdateCrc32(this._checksum, buffer, offset, count);
            this._baseStream.Write(buffer, offset, count);
            this._position += (long)count;
        }

        public override void Flush()
        {
            this.ThrowIfDisposed();
            this._baseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this._isDisposed)
            {
                if (!this._everWritten)
                {
                    this._initialPosition = this._baseBaseStream.Position;
                }

                if (!this._leaveOpenOnClose)
                {
                    this._baseStream.Close();
                }

                if (this._saveCrcAndSizes != null)
                {
                    this._saveCrcAndSizes(this._initialPosition, this.Position, this._checksum);
                }

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
    }
}
