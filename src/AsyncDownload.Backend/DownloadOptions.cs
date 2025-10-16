namespace AsyncDownload.Backend;

public class DownloadOptions
{
    /// <summary>
    /// How many files can be downloaded concurrently.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; } = 3;

    /// <summary>
    /// Allows the downloader to stop gracefully when the application is stopping.
    /// </summary>
    public CancellationToken StopToken { get; set; } = CancellationToken.None;
}