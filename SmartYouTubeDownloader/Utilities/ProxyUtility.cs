using System;

namespace SmartYouTubeDownloader.Utilities;

public static class ProxyUtility
{
    public static string BuildProxyUrl(string host, int port, string? user, string? password)
    {
        if (string.IsNullOrWhiteSpace(host) || port <= 0)
        {
            throw new ArgumentException("Proxy host and port are required.");
        }

        if (!string.IsNullOrEmpty(user))
        {
            var encodedUser = Uri.EscapeDataString(user);
            var encodedPassword = password is null ? string.Empty : Uri.EscapeDataString(password);
            var credentials = string.IsNullOrEmpty(encodedPassword) ? encodedUser : $"{encodedUser}:{encodedPassword}";
            return $"http://{credentials}@{host}:{port}";
        }

        return $"http://{host}:{port}";
    }
}
