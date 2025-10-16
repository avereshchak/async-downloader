using AsyncDownload.Backend.Interfaces;

namespace AsyncDownload.Backend.Implementation;

internal class DownloadManager : IDownloadManager
{
    private readonly IJobStore store;
    private readonly IDownloadQueue queue;

    public DownloadManager(IJobStore store, IDownloadQueue queue)
    {
        this.store = store;
        this.queue = queue;
    }

    public async Task ScheduleDownloadAsync(string url, string filePath, CancellationToken ct)
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

        var job = await store.AddDownloadJobAsync(url, filePath);
        await queue.EnqueueAsync(job, ct);
    }

    public Task<IEnumerable<IJob>> GetAllJobsAsync()
    {
        return store.GetAllAsync();
    }
}
