using System;

namespace SmartYouTubeDownloader.Models;

public sealed class DownloadRequest
{
    public required string Url { get; init; }
    public required string OutputDirectory { get; init; }
    public string OutputTemplate { get; init; } = "%(title)s [%(id)s].%(ext)s";
    public DownloadMode Mode { get; init; } = DownloadMode.Video;
    public string? FormatId { get; init; }
    public int? MaxHeight { get; init; }
    public bool CompatibilityMode { get; init; }
    public bool UseProxy { get; init; }
    public string? ProxyAddress { get; init; }
    public VideoMetadata? Metadata { get; init; }
    public CollisionStrategy CollisionStrategy { get; init; } = CollisionStrategy.AutoRename;
}
