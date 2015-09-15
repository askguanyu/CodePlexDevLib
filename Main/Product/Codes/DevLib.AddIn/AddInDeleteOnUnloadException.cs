//-----------------------------------------------------------------------
// <copyright file="AddInDeleteOnUnloadException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    /// Class AddInDeleteOnUnloadException.
    /// </summary>
    [Serializable]
    public class AddInDeleteOnUnloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDeleteOnUnloadException" /> class.
        /// </summary>
        /// <param name="message">Message string.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AddInDeleteOnUnloadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
