using System;

namespace SmartYouTubeDownloader.Models
{
    public sealed class VideoFormat
    {
        public string Id { get; set; } = string.Empty;
        public int? Height { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string VideoCodec { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public bool HasVideo
        {
            get { return !string.IsNullOrWhiteSpace(VideoCodec) && VideoCodec != "none"; }
        }

        public bool HasAudio
        {
            get { return !string.IsNullOrWhiteSpace(AudioCodec) && AudioCodec != "none"; }
        }

        public string DisplayLabel
        {
            get
            {
                var height = Height.HasValue ? string.Format("{0}p", Height) : "Audio";
                var container = string.IsNullOrWhiteSpace(Extension) ? "?" : Extension;
                var v = HasVideo ? VideoCodec : "-";
                var a = HasAudio ? AudioCodec : "-";
                return string.Format("{0} • {1} • v:{2} a:{3}", height, container, v, a);
            }
        }

        public override string ToString()
        {
            return DisplayLabel;
        }
    }
}
