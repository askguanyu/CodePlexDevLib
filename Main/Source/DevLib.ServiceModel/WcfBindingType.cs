//-----------------------------------------------------------------------
// <copyright file="WcfBindingType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// Wcf Binding type.
    /// </summary>
    public enum WcfBindingType
    {
        /// <summary>
        /// Represents BasicHttpBinding.
        /// </summary>
        BasicHttp,

        /// <summary>
        /// Represents WSHttpBinding.
        /// </summary>
        WSHttp,

        /// <summary>
        /// Represents WSDualHttpBinding.
        /// </summary>
        WSDualHttp,

        /// <summary>
        /// Represents WSFederationHttpBinding.
        /// </summary>
        WSFederationHttp,

        /// <summary>
        /// Represents WS2007HttpBinding.
        /// </summary>
        WS2007Http,

        /// <summary>
        /// Represents WS2007FederationHttpBinding.
        /// </summary>
        WS2007FederationHttp,

        /// <summary>
        /// Represents NetTcpBinding.
        /// </summary>
        NetTcp,

        /// <summary>
        /// Represents NetNamedPipeBinding.
        /// </summary>
        NetNamedPipe,

        /// <summary>
        /// Represents NetMsmqBinding.
        /// </summary>
        NetMsmq,

        /// <summary>
        /// Represents NetPeerTcpBinding.
        /// </summary>
        NetPeerTcp,

        /// <summary>
        /// Represents MsmqIntegrationBinding.
        /// </summary>
        MsmqIntegration,

        /// <summary>
        /// Represents WebHttpBinding.
        /// </summary>
        WebHttp,

        /// <summary>
        /// Represents CustomBinding.
        /// </summary>
        Custom,
#if !__MonoCS__
        /// <summary>
        /// Represents BasicHttpContextBinding.
        /// </summary>
        BasicHttpContext,

        /// <summary>
        /// Represents NetTcpContextBinding.
        /// </summary>
        NetTcpContext,

        /// <summary>
        /// Represents WSHttpContextBinding.
        /// </summary>
        WSHttpContext,
#endif
        /// <summary>
        /// Represents MexHttpBinding.
        /// </summary>
        MexHttp,

        /// <summary>
        /// Represents MexHttpsBinding.
        /// </summary>
        MexHttps,

        /// <summary>
        /// Represents MexTcpBinding.
        /// </summary>
        MexTcp,

        /// <summary>
        /// Represents MexNamedPipeBinding.
        /// </summary>
        MexNamedPipe,
    }
}
