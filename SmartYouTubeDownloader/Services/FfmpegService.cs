using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmartYouTubeDownloader.Models;

namespace SmartYouTubeDownloader.Services;

public sealed class FfmpegService
{
    private static readonly Regex TimeRegex = new(@"time=(?<time>\d{2}:\d{2}:\d{2}\.\d+)", RegexOptions.Compiled);
    private readonly string _ffmpegPath;

    public FfmpegService(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    public async Task<string> TranscodeForPremiereAsync(string inputPath, string outputDirectory, VideoMetadata metadata, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var title = Path.GetFileNameWithoutExtension(inputPath);
        var destination = Path.Combine(outputDirectory, $"{title}.premiere.mp4");

        var args = string.Join(' ', new[]
        {
            "-y",
            $"-i \"{inputPath}\"",
            "-c:v libx264",
            "-preset medium",
            "-crf 20",
            "-pix_fmt yuv420p",
            "-vsync cfr",
            "-r 30",
            "-c:a aac",
            "-b:a 160k",
            "-ar 48000",
            "-ac 2",
            "-movflags +faststart",
            $"\"{destination}\""
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start ffmpeg.");
        await ReadTranscodeProgressAsync(process.StandardError, metadata.Duration, progress, cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("ffmpeg failed to transcode the file.");
        }

        return destination;
    }

    private static async Task ReadTranscodeProgressAsync(StreamReader reader, TimeSpan duration, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var match = TimeRegex.Match(line);
            if (match.Success && TimeSpan.TryParse(match.Groups["time"].Value, CultureInfo.InvariantCulture, out var time))
            {
                var percent = duration.TotalSeconds > 0
                    ? 70 + (int)Math.Clamp(time.TotalSeconds / duration.TotalSeconds * 30, 0, 30)
                    : 70;
                progress.Report(new DownloadProgress
                {
                    Percent = Math.Min(100, percent),
                    Phase = "Transcoding",
                    Status = $"Transcoding for Premiere — {time:hh\\:mm\\:ss} / {duration:hh\\:mm\\:ss}",
                    AdditionalDetails = line
                });
            }
        }

        progress.Report(new DownloadProgress
        {
            Percent = 100,
            Phase = "Completed",
            Status = "Transcoding complete"
        });
    }
}
