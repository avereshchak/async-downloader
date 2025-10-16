namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// Processes download jobs.
/// </summary>
internal interface IJobProcessor
{
    /// <summary>
    /// Processes a download job asynchronously.
    /// </summary>
    Task ProcessAsync(IJob job);
}
