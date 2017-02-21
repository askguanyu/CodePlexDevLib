//-----------------------------------------------------------------------
// <copyright file="AutoIncrementId.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
    using System.Threading;

    /// <summary>
    /// Class AutoIncrementId.
    /// </summary>
    public class AutoIncrementId
    {
        /// <summary>
        /// The next identifier.
        /// </summary>
        private long _nextId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoIncrementId"/> class.
        /// </summary>
        public AutoIncrementId()
            : this(0, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoIncrementId"/> class.
        /// </summary>
        /// <param name="seed">The starting or seed value.</param>
        /// <param name="step">The increment value.</param>
        public AutoIncrementId(long seed = 0, long step = 1)
        {
            this.Seed = seed;
            this._nextId = seed;
            this.Step = step;
        }

        /// <summary>
        /// Gets the seed.
        /// </summary>
        public long Seed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the increment step value.
        /// </summary>
        public long Step
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the next increment Id.
        /// </summary>
        /// <returns>The increment Id.</returns>
        public long Next()
        {
            try
            {
                return Interlocked.Read(ref this._nextId);
            }
            finally
            {
                Interlocked.Add(ref this._nextId, this.Step);
            }
        }

        /// <summary>
        /// Gets the last increment Id.
        /// </summary>
        /// <returns>The last Id.</returns>
        public long Peek()
        {
            return Interlocked.Read(ref this._nextId);
        }
    }
}
