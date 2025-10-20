using System;

namespace SmartYouTubeDownloader.Models;

public sealed class DownloadRequest
{
    public string Url { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string OutputTemplate { get; set; } = "%(title)s [%(id)s].%(ext)s";
    public DownloadMode Mode { get; set; } = DownloadMode.Video;
    public string? FormatId { get; set; }
    public int? MaxHeight { get; set; }
    public bool CompatibilityMode { get; set; }
    public bool UseProxy { get; set; }
    public string? ProxyAddress { get; set; }
    public VideoMetadata? Metadata { get; set; }
    public CollisionStrategy CollisionStrategy { get; set; } = CollisionStrategy.AutoRename;
}
