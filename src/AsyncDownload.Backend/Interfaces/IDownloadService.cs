namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// Downloads content from a URL.
/// </summary>
internal interface IDownloadService
{
    /// <summary>
    /// Downloads content from the specified URL.
    /// </summary>
    Task<Stream> DownloadAsync(Guid jobId, string url, CancellationToken cancellationToken);
}