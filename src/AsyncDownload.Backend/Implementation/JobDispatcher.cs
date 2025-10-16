using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class JobDispatcher
{
    private readonly ILogger<JobDispatcher> logger;
    private readonly IDownloadService downloadService;
    private readonly IFileSystem fileSystem;
    private readonly IJobStore store;
    private readonly SemaphoreSlim semaphore;
    private readonly CancellationToken ct;

    public JobDispatcher(
        IOptions<DownloadOptions> options,
        IJobStore store,
        ILogger<JobDispatcher> logger,
        IDownloadService downloadService,
        IFileSystem fileSystem)
    {
        var maxConcurrentJobs = options.Value.MaxConcurrentDownloads;

        if (maxConcurrentJobs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentJobs), "Must be positive.");
        }

        ct = options.Value.StopToken;

        semaphore = new SemaphoreSlim(maxConcurrentJobs, maxConcurrentJobs);

        this.store = store;
        this.logger = logger;
        this.downloadService = downloadService;
        this.fileSystem = fileSystem;

        // Create a long-running task which monitors the incoming download requests
        // and spawns download tasks as needed.
        Task.Factory.StartNew(ProducerFunc, TaskCreationOptions.LongRunning);
        logger.StartingJobDispatcher(maxConcurrentJobs);
    }

    private async Task ProducerFunc()
    {
        logger.JobDispatcherProducerThreadStarted();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                logger.WaitingForJob();
                var job = await store.DequeueAsync();

                // Do not spawn more download tasks than configured.
                logger.WaitForAvailableJobSlot(job.Id, job.Url);
                await semaphore.WaitAsync(ct);

                logger.AcquiredTheJobSlot(job.Id, job.Url);

                _ = Task.Run(() =>
                {
                    return RunJobAsync(job)
                        .ContinueWith(_ =>
                        {
                            logger.ReleasingJobSlot(job.Id, job.Url);
                            semaphore.Release();
                        }, ct);
                }, ct);
            }
            logger.StoppingJobDispatcher();
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            // Graceful shutdown.
            logger.StoppingJobDispatcher();
        }
        catch (Exception ex)
        {
            logger.JobDispatcherProducerThreadFailed(ex);
            throw;
        }
    }

    private async Task RunJobAsync(IJob job)
    {
        try
        {
            logger.StartingJob(job.Id, job.Url);
            await store.MarkAsStarted(job.Id);

            await using var stream = await downloadService.DownloadAsync(job.Id, job.Url, ct);
            await fileSystem.SaveToFileAsync(stream, job.Id, job.FilePath, ct);
            
            await store.MarkAsCompletedAsync(job.Id);
            logger.JobCompleted(job.Id, job.Url);
        }
        catch (Exception ex)
        {
            logger.JobFailed(ex, job.Id, job.Url);
            await store.MarkAsFailed(job.Id, ex.Message);
        }
    }
}
