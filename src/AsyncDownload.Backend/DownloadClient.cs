using System.Diagnostics;
using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend;

public class DownloadClient : IDisposable, IDownloadManager
{
    private readonly ServiceProvider serviceProvider;
    private readonly IDownloadManager manager;

    public DownloadClient(int maxConcurrentDownloads)
    {
        var container = new ServiceCollection();
        container.AddAsyncDownload(maxConcurrentDownloads);
        serviceProvider = container.BuildServiceProvider();
        serviceProvider.ActivateAsyncDownload();
        manager = serviceProvider.GetRequiredService<IDownloadManager>();
    }

    public void Dispose()
    {
        serviceProvider.Dispose();
    }

    public async Task ScheduleDownloadAsync(string url, string filePath, CancellationToken ct)
    {
        await manager.ScheduleDownloadAsync(url, filePath, ct).ConfigureAwait(false);
    }

    public async Task<IEnumerable<IJob>> GetAllJobsAsync()
    {
        return await manager.GetAllJobsAsync().ConfigureAwait(false);
    }

    public async Task WaitForCompletionAsync(TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        var jobs = await GetAllJobsAsync().ConfigureAwait(false);
        var enumerable = jobs.ToList();
        while (stopwatch.Elapsed < timeout)
        {
            if (enumerable.All(j => j.Status is DownloadStatus.DownloadedSuccessfully or DownloadStatus.DownloadFailed))
            {
                return;
            }

            await Task.Delay(100).ConfigureAwait(false);
        }
    }
}