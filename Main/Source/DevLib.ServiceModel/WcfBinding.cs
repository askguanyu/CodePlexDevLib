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
                return new BasicHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
            }
        }

        /// <summary>
        /// Gets the WSHttpBinding instance.
        /// </summary>
        public static WSHttpBinding WSHttp
        {
            get
            {
                return new WSHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
            }
        }

        /// <summary>
        /// Gets the WSDualHttpBinding instance.
        /// </summary>
        public static WSDualHttpBinding WSDualHttp
        {
            get
            {
                return new WSDualHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSFederationHttpBinding instance.
        /// </summary>
        public static WSFederationHttpBinding WSFederationHttp
        {
            get
            {
                return new WSFederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WS2007HttpBinding instance.
        /// </summary>
        public static WS2007HttpBinding WS2007Http
        {
            get
            {
                return new WS2007HttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
            }
        }

        /// <summary>
        /// Gets the WS2007FederationHttpBinding instance.
        /// </summary>
        public static WS2007FederationHttpBinding WS2007FederationHttp
        {
            get
            {
                return new WS2007FederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetTcpBinding instance.
        /// </summary>
        public static NetTcpBinding NetTcp
        {
            get
            {
                return new NetTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ListenBacklog = int.MaxValue, MaxConnections = ushort.MaxValue, PortSharingEnabled = true, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetNamedPipeBinding instance.
        /// </summary>
        public static NetNamedPipeBinding NetNamedPipe
        {
            get
            {
                return new NetNamedPipeBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, MaxConnections = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetMsmqBinding instance.
        /// </summary>
        public static NetMsmqBinding NetMsmq
        {
            get
            {
                return new NetMsmqBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the NetPeerTcpBinding instance.
        /// </summary>
        public static NetPeerTcpBinding NetPeerTcp
        {
            get
            {
                return new NetPeerTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the MsmqIntegrationBinding instance.
        /// </summary>
        public static MsmqIntegrationBinding MsmqIntegration
        {
            get
            {
                return new MsmqIntegrationBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxReceivedMessageSize = int.MaxValue };
            }
        }

        /// <summary>
        /// Gets the WebHttpBinding instance.
        /// </summary>
        public static WebHttpBinding WebHttp
        {
            get
            {
                return new WebHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
            }
        }

        /// <summary>
        /// Gets the CustomBinding instance.
        /// </summary>
        public static CustomBinding Custom
        {
            get
            {
                return new CustomBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15) };
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
                return new BasicHttpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
            }
        }

        /// <summary>
        /// Gets the NetTcpContextBinding instance.
        /// </summary>
        public static NetTcpContextBinding NetTcpContext
        {
            get
            {
                return new NetTcpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ListenBacklog = int.MaxValue, MaxConnections = ushort.MaxValue, PortSharingEnabled = true, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }
        }

        /// <summary>
        /// Gets the WSHttpContextBinding instance.
        /// </summary>
        public static WSHttpContextBinding WSHttpContext
        {
            get
            {
                return new WSHttpContextBinding() { OpenTimeout = TimeSpan.FromMinutes(15), CloseTimeout = TimeSpan.FromMinutes(15), SendTimeout = TimeSpan.FromMinutes(15), ReceiveTimeout = TimeSpan.FromMinutes(15), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max, AllowCookies = true };
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

                result.SetTimeout(TimeSpan.FromMinutes(15));

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

                result.SetTimeout(TimeSpan.FromMinutes(15));

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

                result.SetTimeout(TimeSpan.FromMinutes(15));

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

                result.SetTimeout(TimeSpan.FromMinutes(15));

                return result;
            }
        }

        /// <summary>
        /// Gets the Binding instance according to a Binding type.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <returns>Instance of Binding.</returns>
        public static Binding GetBinding(Type bindingType)
        {
            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                return (Binding)null;
            }

            return GetBinding(bindingType.Name);
        }

        /// <summary>
        /// Gets the Binding instance according to a Binding type name.
        /// </summary>
        /// <param name="bindingTypeName">The name of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <returns>Instance of Binding.</returns>
        public static Binding GetBinding(string bindingTypeName)
        {
            if (bindingTypeName.Equals("BasicHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.BasicHttp;
            }

            if (bindingTypeName.Equals("WSHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WSHttp;
            }

            if (bindingTypeName.Equals("WSDualHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WSDualHttp;
            }

            if (bindingTypeName.Equals("WSFederationHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WSFederationHttp;
            }

            if (bindingTypeName.Equals("WS2007HttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WS2007Http;
            }

            if (bindingTypeName.Equals("WS2007FederationHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WS2007FederationHttp;
            }

            if (bindingTypeName.Equals("NetTcpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.NetTcp;
            }

            if (bindingTypeName.Equals("NetNamedPipeBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.NetNamedPipe;
            }

            if (bindingTypeName.Equals("NetMsmqBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.NetMsmq;
            }

            if (bindingTypeName.Equals("NetPeerTcpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.NetPeerTcp;
            }

            if (bindingTypeName.Equals("MsmqIntegrationBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.MsmqIntegration;
            }

            if (bindingTypeName.Equals("WebHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WebHttp;
            }

            if (bindingTypeName.Equals("CustomBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.Custom;
            }
#if !__MonoCS__
            if (bindingTypeName.Equals("BasicHttpContextBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.BasicHttpContext;
            }

            if (bindingTypeName.Equals("NetTcpContextBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.NetTcpContext;
            }

            if (bindingTypeName.Equals("WSHttpContextBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.WSHttpContext;
            }
#endif
            if (bindingTypeName.Equals("mexHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.MexHttp;
            }

            if (bindingTypeName.Equals("mexHttpsBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.MexHttps;
            }

            if (bindingTypeName.Equals("mexTcpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.MexTcp;
            }

            if (bindingTypeName.Equals("mexNamedPipeBinding", StringComparison.OrdinalIgnoreCase))
            {
                return WcfBinding.MexNamedPipe;
            }

            try
            {
                Binding result = (Binding)Activator.CreateInstance(Type.GetType(bindingTypeName, true, true));

                result.SetTimeout(TimeSpan.FromMinutes(15));

                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return null;
        }

        /// <summary>
        /// Gets the Binding instance according to a Binding type.
        /// </summary>
        /// <param name="bindingType">Type of the binding.</param>
        /// <returns>Instance of Binding.</returns>
        public static Binding GetBinding(WcfBindingType bindingType)
        {
            switch (bindingType)
            {
                case WcfBindingType.BasicHttp:
                    return WcfBinding.BasicHttp;
                case WcfBindingType.WSHttp:
                    return WcfBinding.WSHttp;
                case WcfBindingType.WSDualHttp:
                    return WcfBinding.WSDualHttp;
                case WcfBindingType.WSFederationHttp:
                    return WcfBinding.WSFederationHttp;
                case WcfBindingType.WS2007Http:
                    return WcfBinding.WS2007Http;
                case WcfBindingType.WS2007FederationHttp:
                    return WcfBinding.WS2007FederationHttp;
                case WcfBindingType.NetTcp:
                    return WcfBinding.NetTcp;
                case WcfBindingType.NetNamedPipe:
                    return WcfBinding.NetNamedPipe;
                case WcfBindingType.NetMsmq:
                    return WcfBinding.NetMsmq;
                case WcfBindingType.NetPeerTcp:
                    return WcfBinding.NetPeerTcp;
                case WcfBindingType.MsmqIntegration:
                    return WcfBinding.MsmqIntegration;
                case WcfBindingType.WebHttp:
                    return WcfBinding.WebHttp;
                case WcfBindingType.Custom:
                    return WcfBinding.Custom;
#if !__MonoCS__
                case WcfBindingType.BasicHttpContext:
                    return WcfBinding.BasicHttpContext;
                case WcfBindingType.NetTcpContext:
                    return WcfBinding.NetTcpContext;
                case WcfBindingType.WSHttpContext:
                    return WcfBinding.WSHttpContext;
#endif
                case WcfBindingType.MexHttp:
                    return WcfBinding.MexHttp;
                case WcfBindingType.MexHttps:
                    return WcfBinding.MexHttps;
                case WcfBindingType.MexTcp:
                    return WcfBinding.MexTcp;
                case WcfBindingType.MexNamedPipe:
                    return WcfBinding.MexNamedPipe;
                default:
                    try
                    {
                        Binding result = (Binding)Activator.CreateInstance(Type.GetType(bindingType.ToString() + "Binding", true, true));

                        result.SetTimeout(TimeSpan.FromMinutes(15));

                        return result;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return null;
                    }
            }
        }

        /// <summary>
        /// Sets binding Open/Close/Send/Receive timeout.
        /// </summary>
        /// <param name="source">The binding instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The source binding instance.</returns>
        public static Binding SetTimeout(this Binding source, TimeSpan timeout)
        {
            if (source != null)
            {
                source.OpenTimeout = timeout;
                source.CloseTimeout = timeout;
                source.SendTimeout = timeout;
                source.ReceiveTimeout = timeout;
            }

            return source;
        }
    }
}
