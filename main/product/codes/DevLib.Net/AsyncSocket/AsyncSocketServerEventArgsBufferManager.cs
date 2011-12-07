//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServerEventArgsBufferManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System.Collections.Concurrent;
    using System.Net.Sockets;

    /// <summary>
    /// This class creates a single large buffer which can be divided up and assigned to SocketAsyncEventArgs objects for use with each socket I/O operation.
    /// This enables bufffers to be easily reused and gaurds against fragmenting heap memory.
    ///
    /// The operations exposed on the class are not thread safe.
    /// </summary>
    internal class AsyncSocketServerEventArgsBufferManager
    {
        /// <summary>
        /// the total number of bytes controlled by the buffer pool
        /// </summary>
        private int _numBytes;

        /// <summary>
        /// the underlying byte array maintained by the Buffer Manager
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        ///
        /// </summary>
        private ConcurrentStack<int> _freeIndexPool;

        /// <summary>
        ///
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// Constructor of BufferManager class
        /// </summary>
        /// <param name="totalBytes">Total Bytes</param>
        /// <param name="bufferSize">Buffer Size</param>
        public AsyncSocketServerEventArgsBufferManager(int totalBytes, int bufferSize)
        {
            this._numBytes = totalBytes;
            this._currentIndex = 0;
            this._bufferSize = bufferSize;
            this._freeIndexPool = new ConcurrentStack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer pool
        /// </summary>
        public void InitBuffer()
        {
            // create one big large buffer and divide that out to each SocketAsyncEventArg object
            this._buffer = new byte[this._numBytes];
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object
        /// </summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (this._freeIndexPool.Count > 0)
            {
                int offset;
                if (this._freeIndexPool.TryPop(out offset))
                {
                    args.SetBuffer(this._buffer, offset, this._bufferSize);
                }
            }
            else
            {
                if ((this._numBytes - this._bufferSize) < this._currentIndex)
                {
                    return false;
                }

                args.SetBuffer(this._buffer, this._currentIndex, this._bufferSize);
                this._currentIndex += this._bufferSize;
            }

            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.
        /// This frees the buffer back to the buffer pool
        /// </summary>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            this._freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
