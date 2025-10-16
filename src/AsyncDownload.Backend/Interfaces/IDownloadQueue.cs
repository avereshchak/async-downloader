namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// A queue for managing download jobs.
/// </summary>
internal interface IDownloadQueue
{
    /// <summary>
    /// Enqueue a new download job.
    /// </summary>
    Task EnqueueAsync(IJob job, CancellationToken ct);

    /// <summary>
    /// Dequeue a download job.
    /// </summary>
    Task<IJob> DequeueAsync(CancellationToken ct);
}
