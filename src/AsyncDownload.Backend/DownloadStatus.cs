namespace AsyncDownload.Backend;

/// <summary>
/// Download job statuses.
/// </summary>
public enum DownloadStatus
{
    Queued,
    InProgress,
    DownloadedSuccessfully,
    DownloadFailed
}