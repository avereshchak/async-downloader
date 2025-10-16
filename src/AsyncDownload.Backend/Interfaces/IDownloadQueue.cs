namespace AsyncDownload.Backend.Interfaces;

internal interface IDownloadQueue
{
    Task EnqueueAsync(IJob job, CancellationToken ct);
    Task<IJob> DequeueAsync(CancellationToken ct);
}
