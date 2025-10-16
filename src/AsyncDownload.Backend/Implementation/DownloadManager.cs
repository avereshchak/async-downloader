using AsyncDownload.Backend.Interfaces;

namespace AsyncDownload.Backend.Implementation;

internal class DownloadManager : IDownloadManager
{
    private readonly IJobStore store;

    public DownloadManager(IJobStore store)
    {
        this.store = store;
    }

    public async Task ScheduleDownloadAsync(string url, string filePath)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("The provided URL is not valid.", nameof(url));
        }

        // Validate file path (basic check).
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("The provided file path is not valid.", nameof(filePath));
        }

        await store.EnqueueDownloadAsync(url, filePath);
    }

    public Task<IEnumerable<IJob>> GetAllJobsAsync()
    {
        return store.GetAllAsync();
    }
}