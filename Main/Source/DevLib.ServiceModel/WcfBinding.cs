//-----------------------------------------------------------------------
// <copyright file="WcfBinding.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.MsmqIntegration;
    using System.Xml;

    /// <summary>
    /// Gets Binding instance.
    /// </summary>
    public static class WcfBinding
    {
        /// <summary>
        /// Gets the BasicHttpBinding instance.
        /// </summary>
        public static BasicHttpBinding BasicHttp
        {
            get
            {
                return new BasicHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSHttpBinding instance.
        /// </summary>
        public static WSHttpBinding WSHttp
        {
            get
            {
                return new WSHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSDualHttpBinding instance.
        /// </summary>
        public static WSDualHttpBinding WSDualHttp
        {
            get
            {
                return new WSDualHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSFederationHttpBinding instance.
        /// </summary>
        public static WSFederationHttpBinding WSFederationHttp
        {
            get
            {
                return new WSFederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WS2007HttpBinding instance.
        /// </summary>
        public static WS2007HttpBinding WS2007Http
        {
            get
            {
                return new WS2007HttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WS2007FederationHttpBinding instance.
        /// </summary>
        public static WS2007FederationHttpBinding WS2007FederationHttp
        {
            get
            {
                return new WS2007FederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetTcpBinding instance.
        /// </summary>
        public static NetTcpBinding NetTcp
        {
            get
            {
                return new NetTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ListenBacklog = int.MaxValue, MaxConnections = ushort.MaxValue, PortSharingEnabled = true, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetNamedPipeBinding instance.
        /// </summary>
        public static NetNamedPipeBinding NetNamedPipe
        {
            get
            {
                return new NetNamedPipeBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, MaxConnections = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetMsmqBinding instance.
        /// </summary>
        public static NetMsmqBinding NetMsmq
        {
            get
            {
                return new NetMsmqBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetPeerTcpBinding instance.
        /// </summary>
        public static NetPeerTcpBinding NetPeerTcp
        {
            get
            {
                return new NetPeerTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the MsmqIntegrationBinding instance.
        /// </summary>
        public static MsmqIntegrationBinding MsmqIntegration
        {
            get
            {
                return new MsmqIntegrationBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxReceivedMessageSize = int.MaxValue };
            }
        }

        /// <summary>
        /// Gets the WebHttpBinding instance.
        /// </summary>
        public static WebHttpBinding WebHttp
        {
            get
            {
                return new WebHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the CustomBinding instance.
        /// </summary>
        public static CustomBinding Custom
        {
            get
            {
                return new CustomBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10) };
            }
        }
#if !__MonoCS__
        /// <summary>
        /// Gets the BasicHttpContextBinding instance.
        /// </summary>
        public static BasicHttpContextBinding BasicHttpContext
        {
            get
            {
                return new BasicHttpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetTcpContextBinding instance.
        /// </summary>
        public static NetTcpContextBinding NetTcpContext
        {
            get
            {
                return new NetTcpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ListenBacklog = int.MaxValue, MaxConnections = ushort.MaxValue, PortSharingEnabled = true, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSHttpContextBinding instance.
        /// </summary>
        public static WSHttpContextBinding WSHttpContext
        {
            get
            {
                return new WSHttpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }
#endif
        /// <summary>
        /// Gets the MexHttpBinding instance.
        /// </summary>
        public static Binding MexHttp
        {
            get
            {
                Binding result = MetadataExchangeBindings.CreateMexHttpBinding();

                result.OpenTimeout = TimeSpan.FromMinutes(10);
                result.CloseTimeout = TimeSpan.FromMinutes(10);
                result.SendTimeout = TimeSpan.FromMinutes(10);
                result.ReceiveTimeout = TimeSpan.FromMinutes(10);

                return result;
            }
        }

        /// <summary>
        /// Gets the MexHttpsBinding instance.
        /// </summary>
        public static Binding MexHttps
        {
            get
            {
                Binding result = MetadataExchangeBindings.CreateMexHttpsBinding();

                result.OpenTimeout = TimeSpan.FromMinutes(10);
                result.CloseTimeout = TimeSpan.FromMinutes(10);
                result.SendTimeout = TimeSpan.FromMinutes(10);
                result.ReceiveTimeout = TimeSpan.FromMinutes(10);

                return result;
            }
        }

        /// <summary>
        /// Gets the MexTcpBinding instance.
        /// </summary>
        public static Binding MexTcp
        {
            get
            {
                Binding result = MetadataExchangeBindings.CreateMexTcpBinding();

                result.OpenTimeout = TimeSpan.FromMinutes(10);
                result.CloseTimeout = TimeSpan.FromMinutes(10);
                result.SendTimeout = TimeSpan.FromMinutes(10);
                result.ReceiveTimeout = TimeSpan.FromMinutes(10);

                return result;
            }
        }

        /// <summary>
        /// Gets the MexNamedPipeBinding instance.
        /// </summary>
        public static Binding MexNamedPipe
        {
            get
            {
                Binding result = MetadataExchangeBindings.CreateMexNamedPipeBinding();

                result.OpenTimeout = TimeSpan.FromMinutes(10);
                result.CloseTimeout = TimeSpan.FromMinutes(10);
                result.SendTimeout = TimeSpan.FromMinutes(10);
                result.ReceiveTimeout = TimeSpan.FromMinutes(10);

                return result;
            }
        }
    }
}
