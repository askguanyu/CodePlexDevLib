//-----------------------------------------------------------------------
// <copyright file="IPipeFilter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Interface IPipeFilter.
    /// </summary>
    public interface IPipeFilter
    {
        /// <summary>
        /// Gets or sets the pipe filter name.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Processes the specified input PipeMessage.
        /// </summary>
        /// <param name="input">The input PipeMessage.</param>
        /// <returns>The output PipeMessage.</returns>
        PipeMessage Process(PipeMessage input);
    }
}
