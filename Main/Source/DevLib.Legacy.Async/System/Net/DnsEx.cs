using System.Threading.Tasks;

namespace System.Net
{
    public static class DnsEx
    {
        public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress)
        {
            return Task<IPAddress[]>.Factory.FromAsync<string>(new Func<string, AsyncCallback, object, IAsyncResult>(Dns.BeginGetHostAddresses), new Func<IAsyncResult, IPAddress[]>(Dns.EndGetHostAddresses), hostNameOrAddress, null);
        }

        public static Task<IPHostEntry> GetHostEntryAsync(IPAddress address)
        {
            return Task<IPHostEntry>.Factory.FromAsync<IPAddress>(new Func<IPAddress, AsyncCallback, object, IAsyncResult>(Dns.BeginGetHostEntry), new Func<IAsyncResult, IPHostEntry>(Dns.EndGetHostEntry), address, null);
        }

        public static Task<IPHostEntry> GetHostEntryAsync(string hostNameOrAddress)
        {
            return Task<IPHostEntry>.Factory.FromAsync<string>(new Func<string, AsyncCallback, object, IAsyncResult>(Dns.BeginGetHostEntry), new Func<IAsyncResult, IPHostEntry>(Dns.EndGetHostEntry), hostNameOrAddress, null);
        }
    }
}
