//-----------------------------------------------------------------------
// <copyright file="AddInAssemblyCompilerException.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.CodeDom.Compiler;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class AddInAssemblyCompilerException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public AddInAssemblyCompilerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errors"></param>
        public AddInAssemblyCompilerException(string message, CompilerErrorCollection errors)
            : base(message)
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Gets
        /// </summary>
        public CompilerErrorCollection Errors
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Message);

            foreach (CompilerError error in Errors)
            {
                stringBuilder.AppendFormat("{0}\r\n", error);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
