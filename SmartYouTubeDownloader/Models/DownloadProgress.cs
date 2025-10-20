namespace SmartYouTubeDownloader.Models;

public sealed class DownloadProgress
{
    public int Percent { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Phase { get; init; } = string.Empty;
    public double? DownloadRate { get; init; }
    public string? Eta { get; init; }
    public string? AdditionalDetails { get; init; }
}
