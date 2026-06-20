using System.Net;
using System.Net.Sockets;

namespace CS2Toolkit.API;

public static class NetworkAccess
{
    public static int FindAvailablePort(int startPort)
    {
        for (var port = startPort; port < startPort + 100; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }

        return startPort;
    }

    public static bool IsPortAvailable(int port)
    {
        if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Any(endpoint => endpoint.Port == port))
            return false;

        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public static IReadOnlyList<string> GetAccessUrls(int port)
    {
        var urls = new List<string> { $"http://localhost:{port}" };
        foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                urls.Add($"http://{address}:{port}");
        }

        return urls.Distinct().ToList();
    }
}
