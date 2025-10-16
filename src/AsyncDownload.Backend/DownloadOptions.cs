namespace AsyncDownload.Backend;

/// <summary>
/// Options for downloading files.
/// </summary>
public class DownloadOptions
{
    /// <summary>
    /// How many files can be downloaded concurrently.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; } = 3;
}