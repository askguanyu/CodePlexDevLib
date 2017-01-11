using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

public static class AsyncCompatLibExtensions
{
    public static Task<Socket> AcceptSocketAsync(this TcpListener source)
    {
        return Task<Socket>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginAcceptSocket), new Func<IAsyncResult, Socket>(source.EndAcceptSocket), null);
    }

    public static Task<TcpClient> AcceptTcpClientAsync(this TcpListener source)
    {
        return Task<TcpClient>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginAcceptTcpClient), new Func<IAsyncResult, TcpClient>(source.EndAcceptTcpClient), null);
    }

    public static Task AnnounceOfflineTaskAsync(this AnnouncementClient source, EndpointDiscoveryMetadata discoveryMetadata)
    {
        return Task.Factory.FromAsync<EndpointDiscoveryMetadata>(new Func<EndpointDiscoveryMetadata, AsyncCallback, object, IAsyncResult>(source.BeginAnnounceOffline), new Action<IAsyncResult>(source.EndAnnounceOffline), discoveryMetadata, null);
    }

    public static Task AnnounceOnlineTaskAsync(this AnnouncementClient source, EndpointDiscoveryMetadata discoveryMetadata)
    {
        return Task.Factory.FromAsync<EndpointDiscoveryMetadata>(new Func<EndpointDiscoveryMetadata, AsyncCallback, object, IAsyncResult>(source.BeginAnnounceOnline), new Action<IAsyncResult>(source.EndAnnounceOnline), discoveryMetadata, null);
    }

    public static Task AuthenticateAsClientAsync(this NegotiateStream source)
    {
        return Task.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsClient), new Action<IAsyncResult>(source.EndAuthenticateAsClient), null);
    }

    public static Task AuthenticateAsClientAsync(this NegotiateStream source, NetworkCredential credential, string targetName)
    {
        return Task.Factory.FromAsync<NetworkCredential, string>(new Func<NetworkCredential, string, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsClient), new Action<IAsyncResult>(source.EndAuthenticateAsClient), credential, targetName, null);
    }

    public static Task AuthenticateAsClientAsync(this NegotiateStream source, NetworkCredential credential, ChannelBinding binding, string targetName)
    {
        return Task.Factory.FromAsync<NetworkCredential, ChannelBinding, string>(new Func<NetworkCredential, ChannelBinding, string, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsClient), new Action<IAsyncResult>(source.EndAuthenticateAsClient), credential, binding, targetName, null);
    }

    public static Task AuthenticateAsClientAsync(this SslStream source, string targetHost)
    {
        return Task.Factory.FromAsync<string>(new Func<string, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsClient), new Action<IAsyncResult>(source.EndAuthenticateAsClient), targetHost, null);
    }

    public static Task AuthenticateAsServerAsync(this NegotiateStream source)
    {
        return Task.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsServer), new Action<IAsyncResult>(source.EndAuthenticateAsServer), null);
    }

    public static Task AuthenticateAsServerAsync(this NegotiateStream source, ExtendedProtectionPolicy policy)
    {
        return Task.Factory.FromAsync<ExtendedProtectionPolicy>(new Func<ExtendedProtectionPolicy, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsServer), new Action<IAsyncResult>(source.EndAuthenticateAsServer), policy, null);
    }

    public static Task AuthenticateAsServerAsync(this NegotiateStream source, NetworkCredential credential, ProtectionLevel requiredProtectionLevel, TokenImpersonationLevel requiredImpersonationLevel)
    {
        return Task.Factory.FromAsync<NetworkCredential, ProtectionLevel, TokenImpersonationLevel>(new Func<NetworkCredential, ProtectionLevel, TokenImpersonationLevel, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsServer), new Action<IAsyncResult>(source.EndAuthenticateAsServer), credential, requiredProtectionLevel, requiredImpersonationLevel, null);
    }

    public static Task AuthenticateAsServerAsync(this SslStream source, X509Certificate serverCertificate)
    {
        return Task.Factory.FromAsync<X509Certificate>(new Func<X509Certificate, AsyncCallback, object, IAsyncResult>(source.BeginAuthenticateAsServer), new Action<IAsyncResult>(source.EndAuthenticateAsServer), serverCertificate, null);
    }

    public static void CancelAfter(this CancellationTokenSource source, int dueTime)
    {
        if (source == null)
        {
            throw new NullReferenceException();
        }
        if (dueTime < -1)
        {
            throw new ArgumentOutOfRangeException("dueTime");
        }
        Timer timer = new Timer(delegate(object self)
        {
            ((IDisposable)self).Dispose();
            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        });
        timer.Change(dueTime, -1);
    }

    public static void CancelAfter(this CancellationTokenSource source, TimeSpan dueTime)
    {
        long num = (long)dueTime.TotalMilliseconds;
        if (num < -1L || num > 2147483647L)
        {
            throw new ArgumentOutOfRangeException("dueTime");
        }
        source.CancelAfter((int)num);
    }

    public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        return new System.Runtime.CompilerServices.ConfiguredTaskAwaitable(task, continueOnCapturedContext);
    }

    public static System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        return new System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult>(task, continueOnCapturedContext);
    }

    public static Task ConnectAsync(this TcpClient source, string hostname, int port)
    {
        return Task.Factory.FromAsync<string, int>(new Func<string, int, AsyncCallback, object, IAsyncResult>(source.BeginConnect), new Action<IAsyncResult>(source.EndConnect), hostname, port, null);
    }

    public static Task ConnectAsync(this TcpClient source, IPAddress address, int port)
    {
        return Task.Factory.FromAsync<IPAddress, int>(new Func<IPAddress, int, AsyncCallback, object, IAsyncResult>(source.BeginConnect), new Action<IAsyncResult>(source.EndConnect), address, port, null);
    }

    public static Task ConnectAsync(this TcpClient source, IPAddress[] ipAddresses, int port)
    {
        return Task.Factory.FromAsync<IPAddress[], int>(new Func<IPAddress[], int, AsyncCallback, object, IAsyncResult>(source.BeginConnect), new Action<IAsyncResult>(source.EndConnect), ipAddresses, port, null);
    }

    public static Task CopyToAsync(this Stream source, Stream destination)
    {
        return source.CopyToAsync(destination, 4096);
    }

    public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize)
    {
        return source.CopyToAsync(destination, bufferSize, CancellationToken.None);
    }

    public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        if (destination == null)
        {
            throw new ArgumentNullException("destination");
        }
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException("bufferSize");
        }
        if (!source.CanRead && !source.CanWrite)
        {
            throw new ObjectDisposedException("source");
        }
        if (!destination.CanRead && !destination.CanWrite)
        {
            throw new ObjectDisposedException("destination");
        }
        if (!source.CanRead)
        {
            throw new NotSupportedException();
        }
        if (!destination.CanWrite)
        {
            throw new NotSupportedException();
        }
        return source.CopyToAsyncInternal(destination, bufferSize, cancellationToken);
    }

    public static Task<byte[]> DownloadDataTaskAsync(this WebClient webClient, string address)
    {
        return webClient.DownloadDataTaskAsync(webClient.GetUri(address));
    }

    public static Task<byte[]> DownloadDataTaskAsync(this WebClient webClient, Uri address)
    {
        TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
        DownloadDataCompletedEventHandler completedHandler = null;
        completedHandler = delegate(object sender, DownloadDataCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<byte[]>(tcs, true, e, () => e.Result, delegate
            {
                webClient.DownloadDataCompleted -= completedHandler;
            });
        };
        webClient.DownloadDataCompleted += completedHandler;
        try
        {
            webClient.DownloadDataAsync(address, tcs);
        }
        catch
        {
            webClient.DownloadDataCompleted -= completedHandler;
            throw;
        }
        return tcs.Task;
    }

    public static Task DownloadFileTaskAsync(this WebClient webClient, string address, string fileName)
    {
        return webClient.DownloadFileTaskAsync(webClient.GetUri(address), fileName);
    }

    public static Task DownloadFileTaskAsync(this WebClient webClient, Uri address, string fileName)
    {
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(address);
        AsyncCompletedEventHandler completedHandler = null;
        completedHandler = delegate(object sender, AsyncCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<object>(tcs, true, e, () => null, delegate
            {
                webClient.DownloadFileCompleted -= completedHandler;
            });
        };
        webClient.DownloadFileCompleted += completedHandler;
        try
        {
            webClient.DownloadFileAsync(address, fileName, tcs);
        }
        catch
        {
            webClient.DownloadFileCompleted -= completedHandler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<string> DownloadStringTaskAsync(this WebClient webClient, string address)
    {
        return webClient.DownloadStringTaskAsync(webClient.GetUri(address));
    }

    public static Task<string> DownloadStringTaskAsync(this WebClient webClient, Uri address)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
        DownloadStringCompletedEventHandler completedHandler = null;
        completedHandler = delegate(object sender, DownloadStringCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<string>(tcs, true, e, () => e.Result, delegate
            {
                webClient.DownloadStringCompleted -= completedHandler;
            });
        };
        webClient.DownloadStringCompleted += completedHandler;
        try
        {
            webClient.DownloadStringAsync(address, tcs);
        }
        catch
        {
            webClient.DownloadStringCompleted -= completedHandler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<int> ExecuteNonQueryAsync(this SqlCommand source)
    {
        return source.ExecuteNonQueryAsync(CancellationToken.None);
    }

    public static Task<int> ExecuteNonQueryAsync(this SqlCommand source, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation<int>(cancellationToken);
        }
        return Task<int>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginExecuteNonQuery), new Func<IAsyncResult, int>(source.EndExecuteNonQuery), null);
    }

    public static Task<SqlDataReader> ExecuteReaderAsync(this SqlCommand source)
    {
        return source.ExecuteReaderAsync(CancellationToken.None);
    }

    public static Task<SqlDataReader> ExecuteReaderAsync(this SqlCommand source, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation<SqlDataReader>(cancellationToken);
        }
        return Task<SqlDataReader>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginExecuteReader), new Func<IAsyncResult, SqlDataReader>(source.EndExecuteReader), null);
    }

    public static Task<XmlReader> ExecuteXmlReaderAsync(this SqlCommand source)
    {
        return source.ExecuteXmlReaderAsync(CancellationToken.None);
    }

    public static Task<XmlReader> ExecuteXmlReaderAsync(this SqlCommand source, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation<XmlReader>(cancellationToken);
        }
        return Task<XmlReader>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginExecuteXmlReader), new Func<IAsyncResult, XmlReader>(source.EndExecuteXmlReader), null);
    }

    public static Task<FindResponse> FindTaskAsync(this DiscoveryClient discoveryClient, FindCriteria criteria)
    {
        if (discoveryClient == null)
        {
            throw new ArgumentNullException("discoveryClient");
        }
        TaskCompletionSource<FindResponse> tcs = new TaskCompletionSource<FindResponse>(discoveryClient);
        EventHandler<FindCompletedEventArgs> completedHandler = null;
        completedHandler = delegate(object sender, FindCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<FindResponse>(tcs, true, e, () => e.Result, delegate
            {
                discoveryClient.FindCompleted -= completedHandler;
            });
        };
        discoveryClient.FindCompleted += completedHandler;
        try
        {
            discoveryClient.FindAsync(criteria, tcs);
        }
        catch
        {
            discoveryClient.FindCompleted -= completedHandler;
            throw;
        }
        return tcs.Task;
    }

    public static Task FlushAsync(this Stream source)
    {
        return source.FlushAsync(CancellationToken.None);
    }

    public static Task FlushAsync(this Stream source, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation(cancellationToken);
        }
        return Task.Factory.StartNew(delegate(object s)
        {
            ((Stream)s).Flush();
        }, source, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task FlushAsync(this TextWriter target)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.Flush();
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static System.Runtime.CompilerServices.TaskAwaiter GetAwaiter(this Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        return new System.Runtime.CompilerServices.TaskAwaiter(task);
    }

    public static System.Runtime.CompilerServices.TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        return new System.Runtime.CompilerServices.TaskAwaiter<TResult>(task);
    }

    public static Task<X509Certificate2> GetClientCertificateAsync(this HttpListenerRequest source)
    {
        return Task<X509Certificate2>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetClientCertificate), new Func<IAsyncResult, X509Certificate2>(source.EndGetClientCertificate), null);
    }

    public static Task<HttpListenerContext> GetContextAsync(this HttpListener source)
    {
        return Task<HttpListenerContext>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetContext), new Func<IAsyncResult, HttpListenerContext>(source.EndGetContext), null);
    }

    public static Task<MetadataSet> GetMetadataAsync(this MetadataExchangeClient source)
    {
        return Task<MetadataSet>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetMetadata), new Func<IAsyncResult, MetadataSet>(source.EndGetMetadata), null);
    }

    public static Task<MetadataSet> GetMetadataAsync(this MetadataExchangeClient source, Uri address, MetadataExchangeClientMode mode)
    {
        return Task<MetadataSet>.Factory.FromAsync<Uri, MetadataExchangeClientMode>(new Func<Uri, MetadataExchangeClientMode, AsyncCallback, object, IAsyncResult>(source.BeginGetMetadata), new Func<IAsyncResult, MetadataSet>(source.EndGetMetadata), address, mode, null);
    }

    public static Task<MetadataSet> GetMetadataAsync(this MetadataExchangeClient source, EndpointAddress address)
    {
        return Task<MetadataSet>.Factory.FromAsync<EndpointAddress>(new Func<EndpointAddress, AsyncCallback, object, IAsyncResult>(source.BeginGetMetadata), new Func<IAsyncResult, MetadataSet>(source.EndGetMetadata), address, null);
    }

    public static Task<Stream> GetRequestStreamAsync(this WebRequest source)
    {
        return Task.Factory.StartNew<Task<Stream>>(() => Task<Stream>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetRequestStream), new Func<IAsyncResult, Stream>(source.EndGetRequestStream), null), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap<Stream>();
    }

    public static Task<WebResponse> GetResponseAsync(this WebRequest source)
    {
        return Task.Factory.StartNew<Task<WebResponse>>(() => Task<WebResponse>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetResponse), new Func<IAsyncResult, WebResponse>(source.EndGetResponse), null), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap<WebResponse>();
    }

    public static Task<UnicastIPAddressInformationCollection> GetUnicastAddressesAsync(this IPGlobalProperties source)
    {
        return Task<UnicastIPAddressInformationCollection>.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(source.BeginGetUnicastAddresses), new Func<IAsyncResult, UnicastIPAddressInformationCollection>(source.EndGetUnicastAddresses), null);
    }

    public static Task InvokeAsync(this Dispatcher dispatcher, Action action)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException("dispatcher");
        }
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }
        TaskCompletionSource<VoidTaskResult> tcs = new TaskCompletionSource<VoidTaskResult>();
        dispatcher.BeginInvoke(new Action(delegate
        {
            try
            {
                action();
                tcs.TrySetResult(default(VoidTaskResult));
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
        }), new object[0]);
        return tcs.Task;
    }

    public static Task<TResult> InvokeAsync<TResult>(this Dispatcher dispatcher, Func<TResult> function)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException("dispatcher");
        }
        if (function == null)
        {
            throw new ArgumentNullException("function");
        }
        TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
        dispatcher.BeginInvoke(new Action(delegate
        {
            try
            {
                TResult result = function();
                tcs.TrySetResult(result);
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
        }), new object[0]);
        return tcs.Task;
    }

    public static Task OpenAsync(this SqlConnection source)
    {
        return source.OpenAsync(CancellationToken.None);
    }

    public static Task OpenAsync(this SqlConnection source, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(delegate
        {
            source.Open();
        }, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task<Stream> OpenReadTaskAsync(this WebClient webClient, string address)
    {
        return webClient.OpenReadTaskAsync(webClient.GetUri(address));
    }

    public static Task<Stream> OpenReadTaskAsync(this WebClient webClient, Uri address)
    {
        TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
        OpenReadCompletedEventHandler handler = null;
        handler = delegate(object sender, OpenReadCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<Stream>(tcs, true, e, () => e.Result, delegate
            {
                webClient.OpenReadCompleted -= handler;
            });
        };
        webClient.OpenReadCompleted += handler;
        try
        {
            webClient.OpenReadAsync(address, tcs);
        }
        catch
        {
            webClient.OpenReadCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<Stream> OpenWriteTaskAsync(this WebClient webClient, string address)
    {
        return webClient.OpenWriteTaskAsync(webClient.GetUri(address), null);
    }

    public static Task<Stream> OpenWriteTaskAsync(this WebClient webClient, Uri address)
    {
        return webClient.OpenWriteTaskAsync(address, null);
    }

    public static Task<Stream> OpenWriteTaskAsync(this WebClient webClient, string address, string method)
    {
        return webClient.OpenWriteTaskAsync(webClient.GetUri(address), method);
    }

    public static Task<Stream> OpenWriteTaskAsync(this WebClient webClient, Uri address, string method)
    {
        TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
        OpenWriteCompletedEventHandler handler = null;
        handler = delegate(object sender, OpenWriteCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<Stream>(tcs, true, e, () => e.Result, delegate
            {
                webClient.OpenWriteCompleted -= handler;
            });
        };
        webClient.OpenWriteCompleted += handler;
        try
        {
            webClient.OpenWriteAsync(address, method, tcs);
        }
        catch
        {
            webClient.OpenWriteCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<int> ReadAsync(this Stream source, byte[] buffer, int offset, int count)
    {
        return source.ReadAsync(buffer, offset, count, CancellationToken.None);
    }

    public static Task<int> ReadAsync(this Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation<int>(cancellationToken);
        }
        return Task<int>.Factory.FromAsync<byte[], int, int>(new Func<byte[], int, int, AsyncCallback, object, IAsyncResult>(source.BeginRead), new Func<IAsyncResult, int>(source.EndRead), buffer, offset, count, null);
    }

    public static Task<int> ReadAsync(this TextReader source, char[] buffer, int index, int count)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        return Task.Factory.StartNew<int>(() => source.Read(buffer, index, count), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task<int> ReadBlockAsync(this TextReader source, char[] buffer, int index, int count)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        return Task.Factory.StartNew<int>(() => source.ReadBlock(buffer, index, count), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task<string> ReadLineAsync(this TextReader source)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        return Task.Factory.StartNew<string>(() => source.ReadLine(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task<string> ReadToEndAsync(this TextReader source)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }
        return Task.Factory.StartNew<string>(() => source.ReadToEnd(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task<ResolveResponse> ResolveTaskAsync(this DiscoveryClient discoveryClient, ResolveCriteria criteria)
    {
        if (discoveryClient == null)
        {
            throw new ArgumentNullException("discoveryClient");
        }
        TaskCompletionSource<ResolveResponse> tcs = new TaskCompletionSource<ResolveResponse>(discoveryClient);
        EventHandler<ResolveCompletedEventArgs> completedHandler = null;
        completedHandler = delegate(object sender, ResolveCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<ResolveResponse>(tcs, true, e, () => e.Result, delegate
            {
                discoveryClient.ResolveCompleted -= completedHandler;
            });
        };
        discoveryClient.ResolveCompleted += completedHandler;
        try
        {
            discoveryClient.ResolveAsync(criteria, tcs);
        }
        catch
        {
            discoveryClient.ResolveCompleted -= completedHandler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<int> SendAsync(this UdpClient source, byte[] datagram, int bytes, IPEndPoint endPoint)
    {
        return Task<int>.Factory.FromAsync<byte[], int, IPEndPoint>(new Func<byte[], int, IPEndPoint, AsyncCallback, object, IAsyncResult>(source.BeginSend), new Func<IAsyncResult, int>(source.EndSend), datagram, bytes, endPoint, null);
    }

    public static Task<int> SendAsync(this UdpClient source, byte[] datagram, int bytes)
    {
        return Task<int>.Factory.FromAsync<byte[], int>(new Func<byte[], int, AsyncCallback, object, IAsyncResult>(source.BeginSend), new Func<IAsyncResult, int>(source.EndSend), datagram, bytes, null);
    }

    public static Task<int> SendAsync(this UdpClient source, byte[] datagram, int bytes, string hostname, int port)
    {
        return Task<int>.Factory.FromAsync((AsyncCallback callback, object state) => source.BeginSend(datagram, bytes, hostname, port, callback, state), new Func<IAsyncResult, int>(source.EndSend), null);
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, IPAddress address)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, address, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(address, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, string hostNameOrAddress)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, hostNameOrAddress, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(hostNameOrAddress, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, IPAddress address, int timeout)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, address, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(address, timeout, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, string hostNameOrAddress, int timeout)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, hostNameOrAddress, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(hostNameOrAddress, timeout, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, IPAddress address, int timeout, byte[] buffer)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, address, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(address, timeout, buffer, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, string hostNameOrAddress, int timeout, byte[] buffer)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, hostNameOrAddress, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(hostNameOrAddress, timeout, buffer, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, IPAddress address, int timeout, byte[] buffer, PingOptions options)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, address, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(address, timeout, buffer, options, tcs);
        });
    }

    public static Task<PingReply> SendTaskAsync(this Ping ping, string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(ping, hostNameOrAddress, delegate(TaskCompletionSource<PingReply> tcs)
        {
            ping.SendAsync(hostNameOrAddress, timeout, buffer, options, tcs);
        });
    }

    public static Task SendTaskAsync(this SmtpClient smtpClient, string from, string recipients, string subject, string body)
    {
        MailMessage message = new MailMessage(from, recipients, subject, body);
        return smtpClient.SendTaskAsync(message);
    }

    public static Task SendTaskAsync(this SmtpClient smtpClient, MailMessage message)
    {
        return AsyncCompatLibExtensions.SendTaskAsyncCore(smtpClient, message, delegate(TaskCompletionSource<object> tcs)
        {
            smtpClient.SendAsync(message, tcs);
        });
    }

    public static Task<byte[]> UploadDataTaskAsync(this WebClient webClient, string address, byte[] data)
    {
        return webClient.UploadDataTaskAsync(webClient.GetUri(address), null, data);
    }

    public static Task<byte[]> UploadDataTaskAsync(this WebClient webClient, Uri address, byte[] data)
    {
        return webClient.UploadDataTaskAsync(address, null, data);
    }

    public static Task<byte[]> UploadDataTaskAsync(this WebClient webClient, string address, string method, byte[] data)
    {
        return webClient.UploadDataTaskAsync(webClient.GetUri(address), method, data);
    }

    public static Task<byte[]> UploadDataTaskAsync(this WebClient webClient, Uri address, string method, byte[] data)
    {
        TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
        UploadDataCompletedEventHandler handler = null;
        handler = delegate(object sender, UploadDataCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<byte[]>(tcs, true, e, () => e.Result, delegate
            {
                webClient.UploadDataCompleted -= handler;
            });
        };
        webClient.UploadDataCompleted += handler;
        try
        {
            webClient.UploadDataAsync(address, method, data, tcs);
        }
        catch
        {
            webClient.UploadDataCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<byte[]> UploadFileTaskAsync(this WebClient webClient, string address, string fileName)
    {
        return webClient.UploadFileTaskAsync(webClient.GetUri(address), null, fileName);
    }

    public static Task<byte[]> UploadFileTaskAsync(this WebClient webClient, Uri address, string fileName)
    {
        return webClient.UploadFileTaskAsync(address, null, fileName);
    }

    public static Task<byte[]> UploadFileTaskAsync(this WebClient webClient, string address, string method, string fileName)
    {
        return webClient.UploadFileTaskAsync(webClient.GetUri(address), method, fileName);
    }

    public static Task<byte[]> UploadFileTaskAsync(this WebClient webClient, Uri address, string method, string fileName)
    {
        TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
        UploadFileCompletedEventHandler handler = null;
        handler = delegate(object sender, UploadFileCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<byte[]>(tcs, true, e, () => e.Result, delegate
            {
                webClient.UploadFileCompleted -= handler;
            });
        };
        webClient.UploadFileCompleted += handler;
        try
        {
            webClient.UploadFileAsync(address, method, fileName, tcs);
        }
        catch
        {
            webClient.UploadFileCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    public static Task<string> UploadStringTaskAsync(this WebClient webClient, string address, string data)
    {
        return webClient.UploadStringTaskAsync(address, null, data);
    }

    public static Task<string> UploadStringTaskAsync(this WebClient webClient, Uri address, string data)
    {
        return webClient.UploadStringTaskAsync(address, null, data);
    }

    public static Task<string> UploadStringTaskAsync(this WebClient webClient, string address, string method, string data)
    {
        return webClient.UploadStringTaskAsync(webClient.GetUri(address), method, data);
    }

    public static Task<string> UploadStringTaskAsync(this WebClient webClient, Uri address, string method, string data)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
        UploadStringCompletedEventHandler handler = null;
        handler = delegate(object sender, UploadStringCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<string>(tcs, true, e, () => e.Result, delegate
            {
                webClient.UploadStringCompleted -= handler;
            });
        };
        webClient.UploadStringCompleted += handler;
        try
        {
            webClient.UploadStringAsync(address, method, data, tcs);
        }
        catch
        {
            webClient.UploadStringCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    public static Task WriteAsync(this Stream source, byte[] buffer, int offset, int count)
    {
        return source.WriteAsync(buffer, offset, count, CancellationToken.None);
    }

    public static Task WriteAsync(this Stream source, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncCompatLibExtensions.FromCancellation(cancellationToken);
        }
        return Task.Factory.FromAsync<byte[], int, int>(new Func<byte[], int, int, AsyncCallback, object, IAsyncResult>(source.BeginWrite), new Action<IAsyncResult>(source.EndWrite), buffer, offset, count, null);
    }

    public static Task WriteAsync(this TextWriter target, string value)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.Write(value);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteAsync(this TextWriter target, char value)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.Write(value);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteAsync(this TextWriter target, char[] buffer)
    {
        return target.WriteAsync(buffer, 0, buffer.Length);
    }

    public static Task WriteAsync(this TextWriter target, char[] buffer, int index, int count)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.Write(buffer, index, count);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteLineAsync(this TextWriter target)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.WriteLine();
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteLineAsync(this TextWriter target, string value)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.WriteLine(value);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteLineAsync(this TextWriter target, char value)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.WriteLine(value);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Task WriteLineAsync(this TextWriter target, char[] buffer)
    {
        return target.WriteLineAsync(buffer, 0, buffer.Length);
    }

    public static Task WriteLineAsync(this TextWriter target, char[] buffer, int index, int count)
    {
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        return Task.Factory.StartNew(delegate
        {
            target.WriteLine(buffer, index, count);
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    private static Task CopyToAsyncInternal(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        byte[] array = new byte[bufferSize];
        int count;
        while ((count = source.ReadAsync(array, 0, array.Length, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult()) > 0)
        {
            destination.WriteAsync(array, 0, count, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        return TaskEx.FromResult(0);
    }

    private static Task FromCancellation(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            throw new ArgumentOutOfRangeException("cancellationToken");
        }
        return new Task(delegate
        {
        }, cancellationToken);
    }

    private static Task<TResult> FromCancellation<TResult>(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            throw new ArgumentOutOfRangeException("cancellationToken");
        }
        return new Task<TResult>(() => default(TResult), cancellationToken);
    }

    private static Uri GetUri(this WebClient webClient, string path)
    {
        string baseAddress = webClient.BaseAddress;
        Uri address;
        if (!string.IsNullOrEmpty(baseAddress))
        {
            if (!Uri.TryCreate(new Uri(baseAddress), path, out address))
            {
                return webClient.GetUri(Path.GetFullPath(path));
            }
        }
        else if (!Uri.TryCreate(path, UriKind.Absolute, out address))
        {
            return webClient.GetUri(Path.GetFullPath(path));
        }
        return webClient.GetUri(address);
    }

    private static Uri GetUri(this WebClient webClient, Uri address)
    {
        if (address == null)
        {
            throw new ArgumentNullException("address");
        }
        Uri uri = address;
        string baseAddress = webClient.BaseAddress;
        if (!address.IsAbsoluteUri && !string.IsNullOrEmpty(baseAddress) && !Uri.TryCreate(webClient.GetUri(baseAddress), address, out uri))
        {
            return address;
        }
        if (uri.Query != null && !(uri.Query == string.Empty))
        {
            return uri;
        }
        StringBuilder stringBuilder = new StringBuilder();
        string str = string.Empty;
        NameValueCollection queryString = webClient.QueryString;
        for (int i = 0; i < queryString.Count; i++)
        {
            stringBuilder.Append(str + queryString.AllKeys[i] + "=" + queryString[i]);
            str = "&";
        }
        return new UriBuilder(uri)
        {
            Query = stringBuilder.ToString()
        }.Uri;
    }

    private static void HandleEapCompletion<T>(TaskCompletionSource<T> tcs, bool requireMatch, AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
    {
        if (requireMatch)
        {
            if (e.UserState != tcs)
            {
                return;
            }
        }
        try
        {
            unregisterHandler();
        }
        finally
        {
            if (e.Cancelled)
            {
                tcs.TrySetCanceled();
            }
            else if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult(getResult());
            }
        }
    }

    private static Task<PingReply> SendTaskAsyncCore(Ping ping, object userToken, Action<TaskCompletionSource<PingReply>> sendAsync)
    {
        if (ping == null)
        {
            throw new ArgumentNullException("ping");
        }
        TaskCompletionSource<PingReply> tcs = new TaskCompletionSource<PingReply>(userToken);
        PingCompletedEventHandler handler = null;
        handler = delegate(object sender, PingCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<PingReply>(tcs, true, e, () => e.Reply, delegate
            {
                ping.PingCompleted -= handler;
            });
        };
        ping.PingCompleted += handler;
        try
        {
            sendAsync(tcs);
        }
        catch
        {
            ping.PingCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }

    private static Task SendTaskAsyncCore(SmtpClient smtpClient, object userToken, Action<TaskCompletionSource<object>> sendAsync)
    {
        if (smtpClient == null)
        {
            throw new ArgumentNullException("smtpClient");
        }
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(userToken);
        SendCompletedEventHandler handler = null;
        handler = delegate(object sender, AsyncCompletedEventArgs e)
        {
            AsyncCompatLibExtensions.HandleEapCompletion<object>(tcs, true, e, () => null, delegate
            {
                smtpClient.SendCompleted -= handler;
            });
        };
        smtpClient.SendCompleted += handler;
        try
        {
            sendAsync(tcs);
        }
        catch
        {
            smtpClient.SendCompleted -= handler;
            throw;
        }
        return tcs.Task;
    }
}
