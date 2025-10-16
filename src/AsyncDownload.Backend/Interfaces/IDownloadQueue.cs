namespace AsyncDownload.Backend.Interfaces;

internal interface IDownloadQueue
{
    Task EnqueueAsync(IJob job);
    Task<IJob> DequeueAsync();
}
