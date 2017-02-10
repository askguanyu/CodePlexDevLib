//-----------------------------------------------------------------------
// <copyright file="Pipeline.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Pipes and Filters Pattern.
    /// </summary>
    public class Pipeline
    {
        /// <summary>
        /// List of filters in the pipeline.
        /// </summary>
        private readonly List<IPipeFilter> _filterChain = new List<IPipeFilter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class.
        /// </summary>
        public Pipeline()
        {
            this.Name = Utilities.NewSequentialGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pipeline"/> class.
        /// </summary>
        /// <param name="name">The pipeline name.</param>
        public Pipeline(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the pipeline name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the filter chain.
        /// </summary>
        public ReadOnlyCollection<IPipeFilter> FilterChain
        {
            get
            {
                return this._filterChain.AsReadOnly();
            }
        }

        /// <summary>
        /// Registers the specified filter to the pipeline.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <returns>Current Pipeline instance.</returns>
        public Pipeline Register(IPipeFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter", "IPipeFilter instance cannot be null.");
            }

            lock (Utilities.GetSyncRoot(this._filterChain))
            {
                this._filterChain.Add(filter);
            }

            return this;
        }

        /// <summary>
        /// Registers the specified filter to the pipeline.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        /// <param name="filterIndex">The filter at the specified index.</param>
        /// <returns>Current Pipeline instance.</returns>
        public Pipeline RegisterAt(IPipeFilter filter, int filterIndex)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter", "IPipeFilter instance cannot be null.");
            }

            if (filterIndex < 0)
            {
                throw new ArgumentOutOfRangeException("filterIndex", "Index is less than 0.");
            }

            lock (Utilities.GetSyncRoot(this._filterChain))
            {
                if (filterIndex > this._filterChain.Count)
                {
                    filterIndex = this._filterChain.Count;
                }

                this._filterChain.Insert(filterIndex, filter);
            }

            return this;
        }

        /// <summary>
        /// Removes the specified filter from the pipeline.
        /// </summary>
        /// <param name="filter">The filter to remove.</param>
        /// <returns>Current Pipeline instance.</returns>
        public Pipeline Unregister(IPipeFilter filter)
        {
            if (filter != null && this._filterChain.Contains(filter))
            {
                lock (Utilities.GetSyncRoot(this._filterChain))
                {
                    this._filterChain.Remove(filter);
                }
            }

            return this;
        }

        /// <summary>
        /// Removes the specified filter from the pipeline at the specified index.
        /// </summary>
        /// <param name="filterIndex">The filter at the specified index.</param>
        /// <returns>Current Pipeline instance.</returns>
        public Pipeline UnregisterAt(int filterIndex)
        {
            if (filterIndex >= 0)
            {
                lock (Utilities.GetSyncRoot(this._filterChain))
                {
                    if (filterIndex >= this._filterChain.Count)
                    {
                        filterIndex = this._filterChain.Count - 1;
                    }

                    this._filterChain.RemoveAt(filterIndex);
                }
            }

            return this;
        }

        /// <summary>
        /// Removes all filters from the pipeline.
        /// </summary>
        /// <returns>Current Pipeline instance.</returns>
        public Pipeline Clear()
        {
            lock (Utilities.GetSyncRoot(this._filterChain))
            {
                this._filterChain.Clear();
            }

            return this;
        }

        /// <summary>
        /// Pumps the specified input PipeMessage to the pipeline and start to process.
        /// </summary>
        /// <param name="input">The input PipeMessage.</param>
        /// <returns>The final PipeMessage output.</returns>
        public PipeMessage Pump(PipeMessage input)
        {
            var nextInput = input.Clone();

            nextInput.ReceivedAt = DateTime.Now;
            nextInput.SentAt = nextInput.ReceivedAt;
            nextInput.LastPipeline = this.Name;
            nextInput.LastFilter = null;

            lock (Utilities.GetSyncRoot(this._filterChain))
            {
                try
                {
                    foreach (var filter in this._filterChain)
                    {
                        try
                        {
                            var receivedAt = DateTime.Now;
                            nextInput.ReceivedAt = receivedAt;
                            nextInput.LastPipeline = this.Name;

                            this.BeforeFilterProcessing(filter, nextInput);

                            var output = filter.Process(nextInput);
                            output.ReceivedAt = receivedAt;
                            output.SentAt = DateTime.Now;
                            output.LastPipeline = this.Name;
                            output.LastFilter = filter.Name;

                            this.AfterFilterProcessed(filter, output);

                            nextInput = output.Clone();
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }
            }

            return nextInput;
        }

        /// <summary>
        /// Occurs before the filter processing message.
        /// </summary>
        /// <param name="filter">The filter going to process message.</param>
        /// <param name="message">The input message.</param>
        protected virtual void BeforeFilterProcessing(IPipeFilter filter, PipeMessage message)
        {
        }

        /// <summary>
        /// Occurs after the filter processed message.
        /// </summary>
        /// <param name="filter">The filter done processed message.</param>
        /// <param name="message">The output message.</param>
        protected virtual void AfterFilterProcessed(IPipeFilter filter, PipeMessage message)
        {
        }
    }
}
