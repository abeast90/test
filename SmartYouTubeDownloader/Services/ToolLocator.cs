using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SmartYouTubeDownloader.Services;

public sealed class ToolLocator
{
    private static readonly string[] YtDlpNames = { "yt-dlp.exe", "yt-dlp" };
    private static readonly string[] FfmpegNames = { "ffmpeg.exe", "ffmpeg" };

    private readonly string _appToolsDirectory;

    public ToolLocator(string appToolsDirectory)
    {
        _appToolsDirectory = appToolsDirectory;
    }

    public string? FindYtDlp() => FindExecutable(YtDlpNames);
    public string? FindFfmpeg() => FindExecutable(FfmpegNames);

    private string? FindExecutable(IEnumerable<string> candidateNames)
    {
        var searchPaths = new List<string>
        {
            _appToolsDirectory,
            AppContext.BaseDirectory,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartYouTubeDownloader", "tools"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
            "D:\\"
        };

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            searchPaths.AddRange(pathEnv.Split(Path.PathSeparator));
        }

        foreach (var candidate in candidateNames)
        {
            foreach (var path in searchPaths)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    var combined = Directory.Exists(path) ? Path.Combine(path, candidate) : path;
                    if (File.Exists(combined))
                    {
                        return Path.GetFullPath(combined);
                    }
                }
                catch
                {
                    // ignore invalid paths
                }
            }
        }

        return null;
    }
}
