namespace AsyncDownload.Backend;

public enum DownloadJobStatus
{
    Queued,
    InProgress,
    DownloadedSuccessfully,
    DownloadFailed
}