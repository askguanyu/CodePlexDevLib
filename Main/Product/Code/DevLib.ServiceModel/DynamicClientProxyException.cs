//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.ServiceModel.Description;
    using System.Text;

    /// <summary>
    /// Represents errors that occur during execution.
    /// </summary>
    [Serializable]
    public class DynamicClientProxyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DynamicClientProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DynamicClientProxyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets metadata import errors.
        /// </summary>
        public IList<MetadataConversionError> MetadataImportErrors
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets code generation errors.
        /// </summary>
        public IList<MetadataConversionError> CodeGenerationErrors
        {
            get;
            internal set;
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

            if (this.MetadataImportErrors != null)
            {
                stringBuilder.AppendLine("Metadata Import Errors:");
                stringBuilder.AppendLine(DynamicClientProxyFactory.GetErrorsString(this.MetadataImportErrors));
            }

            if (this.CodeGenerationErrors != null)
            {
                stringBuilder.AppendLine("Code Generation Errors:");
                stringBuilder.AppendLine(DynamicClientProxyFactory.GetErrorsString(this.CodeGenerationErrors));
            }

            if (this.CompilerErrors != null)
            {
                stringBuilder.AppendLine("Compiler Errors:");
                stringBuilder.AppendLine(DynamicClientProxyFactory.GetErrorsString(this.CompilerErrors));
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
