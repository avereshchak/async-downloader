namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// A store for managing download jobs.
/// </summary>
internal interface IJobStore
{
    /// <summary>
    /// Gets all jobs in the store.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<IJob>> GetAllAsync();

    /// <summary>
    /// Enqueues a new download job.
    /// </summary>
    Task<IJob> AddDownloadJobAsync(string url, string filePath);

    /// <summary>
    /// Changes the status of a job to InProgress.
    /// </summary>
    Task<bool> MarkAsStarted(Guid jobId);

    /// <summary>
    /// Changes the status of a job to Completed.
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    Task MarkAsCompletedAsync(Guid jobId);

    /// <summary>
    /// Changes the status of a job to Failed with an optional status message.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="statusMessage"></param>
    /// <returns></returns>
    Task MarkAsFailed(Guid jobId, string statusMessage);
}
