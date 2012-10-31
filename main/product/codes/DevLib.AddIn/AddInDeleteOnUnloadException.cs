//-----------------------------------------------------------------------
// <copyright file="AddInDeleteOnUnloadException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class AddInDeleteOnUnloadException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AddInDeleteOnUnloadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
