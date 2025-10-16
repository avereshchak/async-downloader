using System.Diagnostics;
using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend;

public class DownloadClient : IDisposable, IDownloadManager
{
    private readonly CancellationTokenSource cts;
    private readonly ServiceProvider serviceProvider;
    private readonly IDownloadManager manager;

    public DownloadClient(int maxConcurrentDownloads)
    {
        cts = new CancellationTokenSource();
        var container = new ServiceCollection();
        container.AddAsyncDownload(maxConcurrentDownloads, cts.Token);
        serviceProvider = container.BuildServiceProvider();
        serviceProvider.ActivateAsyncDownload();
        manager = serviceProvider.GetRequiredService<IDownloadManager>();
    }

    public void Dispose()
    {
        cts.Cancel();
        serviceProvider.Dispose();
    }

    public Task ScheduleDownloadAsync(string url, string filePath)
    {
        return manager.ScheduleDownloadAsync(url, filePath);
    }

    public Task<IEnumerable<IJob>> GetAllJobsAsync()
    {
        return manager.GetAllJobsAsync();
    }

    public async Task WaitForCompletionAsync(TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        var jobs = await GetAllJobsAsync();
        var enumerable = jobs.ToList();
        while (stopwatch.Elapsed < timeout)
        {
            if (enumerable.All(j => j.Status is DownloadStatus.DownloadedSuccessfully or DownloadStatus.DownloadFailed))
            {
                return;
            }

            await Task.Delay(100);
        }
    }
}