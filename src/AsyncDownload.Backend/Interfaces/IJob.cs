namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// A download job.
/// </summary>
public interface IJob
{
    /// <summary>
    /// A unique identifier for the job.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The URL to download.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The file path to save the downloaded file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The current status of the job.
    /// </summary>
    public DownloadStatus Status { get; }

    /// <summary>
    /// An optional status message providing additional information about the job's status.
    /// </summary>
    public string? StatusMessage { get; }
}
