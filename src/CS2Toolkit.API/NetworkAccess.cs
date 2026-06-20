using System.Net;
using System.Net.Sockets;

namespace CS2Toolkit.API;

public static class NetworkAccess
{
    public static bool TryFindAvailablePort(int startPort, out int port)
    {
        for (var candidate = startPort; candidate < startPort + 100; candidate++)
        {
            if (!IsPortAvailable(candidate))
                continue;

            port = candidate;
            return true;
        }

        port = startPort;
        return false;
    }

    public static int FindAvailablePort(int startPort)
    {
        if (TryFindAvailablePort(startPort, out var port))
            return port;

        throw new InvalidOperationException(
            $"No available TCP port in range {startPort}–{startPort + 99}.");
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

    public static IReadOnlyList<string> GetAccessUrls(int port, bool localhostOnly = true)
    {
        var urls = new List<string> { $"http://localhost:{port}" };
        if (localhostOnly)
            return urls;

        foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                urls.Add($"http://{address}:{port}");
        }

        return urls.Distinct().ToList();
    }
}
