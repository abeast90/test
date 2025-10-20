using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SmartYouTubeDownloader.Models;

namespace SmartYouTubeDownloader.Services;

public sealed class YtDlpService
{
    private static readonly Regex DownloadPercentRegex = new(@"\[download\]\s+(?<percent>\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);
    private readonly string _ytDlpPath;

    public YtDlpService(string ytDlpPath)
    {
        _ytDlpPath = ytDlpPath;
    }

    public async Task<VideoMetadata> FetchMetadataAsync(string url, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            Arguments = $"--dump-json -s \"{url}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start yt-dlp.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
        var json = outputTask.Result;
        if (string.IsNullOrWhiteSpace(json))
        {
            var error = errorTask.Result;
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "yt-dlp returned no data." : error);
        }

        var root = JObject.Parse(json);
        var title = root.Value<string>("title") ?? "Unknown";
        var durationSeconds = root.Value<double?>("duration");
        var duration = durationSeconds.HasValue ? TimeSpan.FromSeconds(durationSeconds.Value) : TimeSpan.Zero;
        var id = root.Value<string>("id") ?? string.Empty;
        var thumb = root.Value<string>("thumbnail") ?? string.Empty;

        var formats = new List<VideoFormat>();
        if (root.TryGetValue("formats", out var formatsToken) && formatsToken is JArray formatsArray)
        {
            foreach (var element in formatsArray)
            {
                var format = new VideoFormat
                {
                    Id = element.Value<string>("format_id") ?? string.Empty,
                    Extension = element.Value<string>("ext") ?? string.Empty,
                    VideoCodec = element.Value<string>("vcodec") ?? string.Empty,
                    AudioCodec = element.Value<string>("acodec") ?? string.Empty,
                    Height = element["height"]?.Type == JTokenType.Integer ? element.Value<int?>("height") : null
                };
                formats.Add(format);
            }
        }

        formats.Sort((a, b) => Nullable.Compare(b.Height, a.Height));

        return new VideoMetadata
        {
            Title = title,
            Duration = duration,
            ThumbnailUrl = thumb,
            Formats = formats,
            Id = id
        };
    }

    public async Task DownloadAsync(DownloadRequest request, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        progress.Report(new DownloadProgress { Percent = 2, Status = "Preparing", Phase = "Pre-checks" });

        Directory.CreateDirectory(request.OutputDirectory);
        var arguments = BuildArguments(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (request.UseProxy && !string.IsNullOrWhiteSpace(request.ProxyAddress))
        {
            startInfo.Environment ??= new System.Collections.Generic.Dictionary<string, string>();
            startInfo.Environment["http_proxy"] = request.ProxyAddress;
            startInfo.Environment["https_proxy"] = request.ProxyAddress;
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start yt-dlp.");
        var stdoutTask = ReadDownloadProgressAsync(process.StandardOutput, progress, cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(stderrTask.Result);
        }

        progress.Report(new DownloadProgress { Percent = 70, Status = "Download complete", Phase = "Completed" });
    }

    private string BuildArguments(DownloadRequest request)
    {
        var outputTemplate = Path.Combine(request.OutputDirectory, request.OutputTemplate);
        var args = new List<string>
        {
            "--newline",
            "-o",
            $"\"{outputTemplate}\""
        };

        if (request.Mode == DownloadMode.AudioOnly)
        {
            args.AddRange(new[] { "-x", "--audio-format", "m4a", "--audio-quality", "0" });
        }

        var formatString = BuildFormatString(request);
        if (!string.IsNullOrWhiteSpace(formatString))
        {
            args.Add("-f");
            args.Add($"\"{formatString}\"");
        }

        if (request.UseProxy && !string.IsNullOrWhiteSpace(request.ProxyAddress))
        {
            args.Add("--proxy");
            args.Add($"\"{request.ProxyAddress}\"");
        }

        if (request.CollisionStrategy == CollisionStrategy.Skip)
        {
            args.Add("--no-overwrites");
        }
        else if (request.CollisionStrategy == CollisionStrategy.Overwrite)
        {
            args.Add("--force-overwrites");
        }

        args.Add($"\"{request.Url}\"");
        return string.Join(' ', args);
    }

    private static string BuildFormatString(DownloadRequest request)
    {
        if (request.Mode == DownloadMode.AudioOnly)
        {
            return "bestaudio/bestaudio[ext=m4a]";
        }

        if (!string.IsNullOrEmpty(request.FormatId))
        {
            return request.FormatId;
        }

        if (request.MaxHeight.HasValue)
        {
            var height = request.MaxHeight.Value;
            return $"bv*[height<={height}]+ba/b[height<={height}]";
        }

        return "bestvideo*+bestaudio/best";
    }

    private async Task ReadDownloadProgressAsync(StreamReader reader, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var match = DownloadPercentRegex.Match(line);
            if (match.Success && double.TryParse(match.Groups["percent"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                var percent = (int)Math.Clamp(5 + value * 0.65, 5, 70);
                progress.Report(new DownloadProgress
                {
                    Percent = percent,
                    Phase = "Downloading",
                    Status = line
                });
            }
        }
    }
}
