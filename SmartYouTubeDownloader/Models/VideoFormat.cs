using System;

namespace SmartYouTubeDownloader.Models;

public sealed class VideoFormat
{
    public string Id { get; init; } = string.Empty;
    public int? Height { get; init; }
    public string Extension { get; init; } = string.Empty;
    public string VideoCodec { get; init; } = string.Empty;
    public string AudioCodec { get; init; } = string.Empty;
    public bool HasVideo => !string.IsNullOrWhiteSpace(VideoCodec) && VideoCodec != "none";
    public bool HasAudio => !string.IsNullOrWhiteSpace(AudioCodec) && AudioCodec != "none";
    public string DisplayLabel
    {
        get
        {
            var height = Height.HasValue ? $"{Height}p" : "Audio";
            var container = string.IsNullOrWhiteSpace(Extension) ? "?" : Extension;
            var v = HasVideo ? VideoCodec : "-";
            var a = HasAudio ? AudioCodec : "-";
            return $"{height} • {container} • v:{v} a:{a}";
        }
    }

    public override string ToString() => DisplayLabel;
}
