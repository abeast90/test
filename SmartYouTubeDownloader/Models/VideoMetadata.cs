using System;
using System.Collections.Generic;

namespace SmartYouTubeDownloader.Models
{
    public sealed class VideoMetadata
    {
        public string Title { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public IReadOnlyList<VideoFormat> Formats { get; set; } = Array.Empty<VideoFormat>();
        public string Id { get; set; } = string.Empty;
    }
}
