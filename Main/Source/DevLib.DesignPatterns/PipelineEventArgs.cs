//-----------------------------------------------------------------------
// <copyright file="PipelineEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;

    /// <summary>
    /// Pipeline EventArgs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    [Serializable]
    public class PipelineEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineEventArgs"/> class.
        /// </summary>
        /// <param name="message">The pipe message.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="pipeFilter">The pipe filter.</param>
        /// <param name="exception">The exception.</param>
        public PipelineEventArgs(PipeMessage message, Pipeline pipeline, IPipeFilter pipeFilter, Exception exception = null)
        {
            this.Message = message;
            this.Pipeline = pipeline;
            this.PipeFilter = pipeFilter;
            this.InnerException = exception;
        }

        /// <summary>
        /// Gets the pipe message.
        /// </summary>
        public PipeMessage Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        public Pipeline Pipeline
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the pipe filter.
        /// </summary>
        public IPipeFilter PipeFilter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the inner exception.
        /// </summary>
        public Exception InnerException
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether has error.
        /// </summary>
        public bool HasError
        {
            get
            {
                return this.InnerException != null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "Pipeline={0}, Filter={1}, HasError={2}, Exception={3}",
                this.Pipeline != null ? this.Pipeline.Name ?? string.Empty : string.Empty,
                this.PipeFilter != null ? this.PipeFilter.Name ?? string.Empty : string.Empty,
                this.HasError.ToString(),
                this.InnerException != null ? this.InnerException.ToString().Replace(Environment.NewLine, " ") : string.Empty);
        }
    }
}
