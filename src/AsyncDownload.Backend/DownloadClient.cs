using System.Diagnostics;
using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend;

/// <summary>
/// A client for scheduling and managing download jobs.
/// </summary>
/// <remarks>Use it in case you don't want adding the services
/// into your DI container.</remarks>
public class DownloadClient : IDisposable, IDownloadManager
{
    private readonly ServiceProvider serviceProvider;
    private readonly IDownloadManager manager;

    /// <summary>
    /// Initializes download services with a specified
    /// maximum number of concurrent downloads.
    /// </summary>
    /// <param name="maxConcurrentDownloads"></param>
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

    /// <summary>
    /// Request a new download job.
    /// </summary>
    public async Task ScheduleDownloadAsync(string url, string filePath, CancellationToken ct)
    {
        await manager.ScheduleDownloadAsync(url, filePath, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Get all download requests.
    /// </summary>
    public async Task<IEnumerable<IJob>> GetAllJobsAsync()
    {
        return await manager.GetAllJobsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Waits until all download jobs are completed or the timeout is reached.
    /// </summary>
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