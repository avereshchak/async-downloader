using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class JobDispatcher : IDisposable
{
    private readonly ILogger<JobDispatcher> logger;
    private readonly IDownloadService downloadService;
    private readonly IFileSystem fileSystem;
    private readonly IJobStore store;
    private readonly IDownloadQueue queue;
    private readonly SemaphoreSlim semaphore;
    private readonly CancellationTokenSource cts;

    public JobDispatcher(
        IOptions<DownloadOptions> options,
        IJobStore store,
        ILogger<JobDispatcher> logger,
        IDownloadService downloadService,
        IFileSystem fileSystem,
        IDownloadQueue queue)
    {
        var maxConcurrentJobs = options.Value.MaxConcurrentDownloads;

        if (maxConcurrentJobs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentJobs), "Must be positive.");
        }

        semaphore = new SemaphoreSlim(maxConcurrentJobs, maxConcurrentJobs);
        cts = new CancellationTokenSource();

        this.store = store;
        this.logger = logger;
        this.downloadService = downloadService;
        this.fileSystem = fileSystem;
        this.queue = queue;

        // Create a long-running task which monitors the incoming download requests
        // and spawns download tasks as needed.
        Task.Factory.StartNew(ProducerFunc, TaskCreationOptions.LongRunning);
        logger.StartingJobDispatcher(maxConcurrentJobs);
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private async Task ProducerFunc()
    {
        logger.JobDispatcherProducerThreadStarted();

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                logger.WaitingForJob();
                var job = await queue.DequeueAsync(cts.Token).ConfigureAwait(false);

                // Do not spawn more download tasks than configured.
                logger.WaitForAvailableJobSlot(job.Id, job.Url);
                await semaphore.WaitAsync(cts.Token).ConfigureAwait(false);

                logger.AcquiredTheJobSlot(job.Id, job.Url);

                _ = Task.Run(() =>
                {
                    return RunJobAsync(job, cts.Token)
                        .ContinueWith(_ =>
                        {
                            logger.ReleasingJobSlot(job.Id, job.Url);
                            semaphore.Release();
                        }, cts.Token);
                }, cts.Token);
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

    private async Task RunJobAsync(IJob job, CancellationToken ct)
    {
        try
        {
            logger.StartingJob(job.Id, job.Url);
            await store.MarkAsStarted(job.Id).ConfigureAwait(false);

            await using var stream = await downloadService.DownloadAsync(job.Id, job.Url, ct);
            
            await fileSystem.SaveToFileAsync(stream, job.Id, job.FilePath, ct)
                .ConfigureAwait(false);
            
            await store.MarkAsCompletedAsync(job.Id).ConfigureAwait(false);
            logger.JobCompleted(job.Id, job.Url);
        }
        catch (Exception ex)
        {
            logger.JobFailed(ex, job.Id, job.Url);
            await store.MarkAsFailed(job.Id, ex.Message).ConfigureAwait(false);
        }
    }
}
