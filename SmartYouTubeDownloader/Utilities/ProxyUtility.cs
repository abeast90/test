using System;

namespace SmartYouTubeDownloader.Utilities
{
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
                var encodedPassword = password == null ? string.Empty : Uri.EscapeDataString(password);
                var credentials = string.IsNullOrEmpty(encodedPassword) ? encodedUser : string.Format("{0}:{1}", encodedUser, encodedPassword);
                return string.Format("http://{0}@{1}:{2}", credentials, host, port);
            }

            return string.Format("http://{0}:{1}", host, port);
        }
    }
}
