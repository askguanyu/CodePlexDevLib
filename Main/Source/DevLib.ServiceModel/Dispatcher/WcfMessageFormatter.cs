//-----------------------------------------------------------------------
// <copyright file="WcfMessageFormatter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// WcfMessage Formatter.
    /// </summary>
    public class WcfMessageFormatter : IClientMessageFormatter, IDispatchMessageFormatter
    {
        /// <summary>
        /// Field _innerClientMessageFormatter.
        /// </summary>
        private readonly IClientMessageFormatter _innerClientMessageFormatter;

        /// <summary>
        /// Field _innerDispatchMessageFormatter.
        /// </summary>
        private readonly IDispatchMessageFormatter _innerDispatchMessageFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageFormatter"/> class.
        /// </summary>
        /// <param name="innerFormatter">The inner formatter.</param>
        public WcfMessageFormatter(IClientMessageFormatter innerFormatter)
        {
            this._innerClientMessageFormatter = innerFormatter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageFormatter"/> class.
        /// </summary>
        /// <param name="innerFormatter">The inner formatter.</param>
        public WcfMessageFormatter(IDispatchMessageFormatter innerFormatter)
        {
            this._innerDispatchMessageFormatter = innerFormatter;
        }

        /// <summary>
        /// Deserializes the reply.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Reply object.</returns>
        public object DeserializeReply(Message message, object[] parameters)
        {
            return this._innerClientMessageFormatter.DeserializeReply(message, parameters);
        }

        /// <summary>
        /// Serializes the request.
        /// </summary>
        /// <param name="messageVersion">The message version.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Request message.</returns>
        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return this._innerClientMessageFormatter.SerializeRequest(messageVersion, parameters);
        }

        /// <summary>
        /// Deserializes the request.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        public void DeserializeRequest(Message message, object[] parameters)
        {
            this._innerDispatchMessageFormatter.DeserializeRequest(message, parameters);
        }

        /// <summary>
        /// Serializes the reply.
        /// </summary>
        /// <param name="messageVersion">The message version.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="result">The result.</param>
        /// <returns>Reply message.</returns>
        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            return this._innerDispatchMessageFormatter.SerializeReply(messageVersion, parameters, result);
        }
    }
}
