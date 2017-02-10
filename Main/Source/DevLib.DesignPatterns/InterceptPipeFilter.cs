//-----------------------------------------------------------------------
// <copyright file="InterceptPipeFilter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Pass through PipeFilter.
    /// </summary>
    public class InterceptPipeFilter : IPipeFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptPipeFilter"/> class.
        /// </summary>
        public InterceptPipeFilter()
        {
            this.Name = "DevLib.DesignPatterns.InterceptPipeFilter";
        }

        /// <summary>
        /// Gets or sets the pipe filter name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Processes the specified input PipeMessage.
        /// </summary>
        /// <param name="input">The input PipeMessage.</param>
        /// <returns>The output PipeMessage.</returns>
        public PipeMessage Process(PipeMessage input)
        {
            return new PipeMessage();
        }
    }
}
