//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxyException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Represents errors that occur during execution.
    /// </summary>
    [Serializable]
    public class WebServiceClientProxyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public WebServiceClientProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public WebServiceClientProxyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets compiler errors.
        /// </summary>
        public IList<CompilerError> CompilerErrors
        {
            get;
            internal set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(base.ToString());

            if (this.CompilerErrors != null)
            {
                stringBuilder.AppendLine("Compiler Errors:");
                stringBuilder.AppendLine(WebServiceClientProxyFactory.GetErrorsString(this.CompilerErrors));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
