using System.Net;
using System.Net.Sockets;

namespace TravelAgency.Api.Services
{
    /// <summary>
    /// Prevents server-side request forgery (SSRF) for endpoints that fetch user-supplied URLs.
    /// The check runs inside the socket connect callback, i.e. on the IP address actually being
    /// connected to, so it also covers redirects and DNS-rebinding tricks.
    /// </summary>
    public static class PublicNetworkGuard
    {
        public static SocketsHttpHandler CreateHandler() => new()
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3,
            ConnectCallback = ConnectToPublicAddressAsync
        };

        private static async ValueTask<Stream> ConnectToPublicAddressAsync(
            SocketsHttpConnectionContext context,
            CancellationToken cancellationToken)
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
            var target = addresses.FirstOrDefault(IsPublic)
                ?? throw new HttpRequestException($"Host '{context.DnsEndPoint.Host}' does not resolve to a public address.");

            var socket = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(new IPEndPoint(target, context.DnsEndPoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        public static bool IsPublic(IPAddress address)
        {
            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            if (IPAddress.IsLoopback(address))
                return false;

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return !(address.IsIPv6LinkLocal
                    || address.IsIPv6SiteLocal
                    || address.IsIPv6Multicast
                    || address.Equals(IPAddress.IPv6None)
                    || address.Equals(IPAddress.IPv6Any)
                    || (address.GetAddressBytes()[0] & 0xFE) == 0xFC); // fc00::/7 unique local
            }

            var b = address.GetAddressBytes();
            return !(b[0] == 0                                   // 0.0.0.0/8
                || b[0] == 10                                    // 10.0.0.0/8
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127)    // 100.64.0.0/10 carrier-grade NAT
                || b[0] == 127                                   // loopback
                || (b[0] == 169 && b[1] == 254)                  // link-local / cloud metadata
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)     // 172.16.0.0/12
                || (b[0] == 192 && b[1] == 168)                  // 192.168.0.0/16
                || b[0] >= 224);                                 // multicast + reserved
        }
    }
}
