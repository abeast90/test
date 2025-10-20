using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmartYouTubeDownloader.Services;

public sealed class ToolInstaller
{
    private readonly HttpClient _httpClient;

    public ToolInstaller(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> DownloadYtDlpAsync(string destinationDirectory, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(destinationDirectory);
        var target = Path.Combine(destinationDirectory, "yt-dlp.exe");
        var uri = new Uri("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe");
        await DownloadFileAsync(uri, target, progress, cancellationToken).ConfigureAwait(false);
        return target;
    }

    public async Task<string> DownloadFfmpegAsync(string destinationDirectory, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(destinationDirectory);
        var tempFile = Path.GetTempFileName();
        var uri = new Uri("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip");
        await DownloadFileAsync(uri, tempFile, progress, cancellationToken).ConfigureAwait(false);

        try
        {
            using var archive = ZipFile.OpenRead(tempFile);
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("/ffmpeg.exe", StringComparison.OrdinalIgnoreCase) ||
                    entry.FullName.EndsWith("\\ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                {
                    var destination = Path.Combine(destinationDirectory, "ffmpeg.exe");
                    entry.ExtractToFile(destination, overwrite: true);
                    return destination;
                }
            }
        }
        finally
        {
            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                // ignore
            }
        }

        throw new InvalidOperationException("ffmpeg.exe not found in archive.");
    }

    private async Task DownloadFileAsync(Uri uri, string destination, IProgress<string>? progress, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var fileStream = File.Create(destination);
        var buffer = new byte[81920];
        long totalRead = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            totalRead += read;
            progress?.Report($"Downloaded {totalRead / 1024.0 / 1024.0:F1} MiB");
        }
    }
}
