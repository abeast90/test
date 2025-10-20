namespace SmartYouTubeDownloader.Models
{
    public sealed class DownloadProgress
    {
        public int Percent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public double? DownloadRate { get; set; }
        public string? Eta { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}
