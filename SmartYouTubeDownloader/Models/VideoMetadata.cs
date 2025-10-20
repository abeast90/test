using System;
using System.Collections.Generic;

namespace SmartYouTubeDownloader.Models;

public sealed class VideoMetadata
{
    public string Title { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public string ThumbnailUrl { get; init; } = string.Empty;
    public IReadOnlyList<VideoFormat> Formats { get; init; } = Array.Empty<VideoFormat>();
    public string Id { get; init; } = string.Empty;
}
