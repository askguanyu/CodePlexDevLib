//-----------------------------------------------------------------------
// <copyright file="PassthroughPipeFilter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Pass through PipeFilter.
    /// </summary>
    public class PassthroughPipeFilter : IPipeFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassthroughPipeFilter"/> class.
        /// </summary>
        public PassthroughPipeFilter()
        {
            this.Name = "DevLib.DesignPatterns.PassthroughPipeFilter";
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
            return input;
        }
    }
}
