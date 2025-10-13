namespace AsyncDownload.Backend;

public class DownloadOptions
{
    public int MaxConcurrentDownloads { get; set; } = 3;
    public CancellationToken AppStoppingToken { get; set; } = CancellationToken.None;
}