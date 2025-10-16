namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// Allows scheduling new downloads and monitoring their status.
/// </summary>
public interface IDownloadManager
{
    /// <summary>
    /// Request a new download job.
    /// </summary>
    Task ScheduleDownloadAsync(string url, string filePath);

    /// <summary>
    /// Get all download requests.
    /// </summary>
    Task<IEnumerable<IJob>> GetAllJobsAsync();
}
