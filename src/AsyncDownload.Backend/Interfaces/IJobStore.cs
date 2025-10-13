namespace AsyncDownload.Backend.Interfaces;

internal interface IJobStore
{
    Task<IEnumerable<IJob>> GetAllAsync();
    Task<IJob> DequeueAsync();
    Task EnqueueDownloadAsync(string url, string filePath);
    Task<bool> StartJobAsync(Guid jobId);
    Task JobCompletedAsync(Guid jobId);
    Task JobFailedAsync(Guid jobId, string statusMessage);
}