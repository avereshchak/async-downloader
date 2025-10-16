using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class JobDispatcher
{
    private readonly ILogger<JobDispatcher> logger;
    private readonly IJobProcessor jobProcessor;
    private readonly IJobStore store;
    private readonly SemaphoreSlim semaphore;
    private readonly CancellationToken ct;

    public JobDispatcher(
        IOptions<DownloadOptions> options,
        IJobStore store,
        ILogger<JobDispatcher> logger,
        IJobProcessor jobProcessor)
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
        this.jobProcessor = jobProcessor;

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
                    return InvokeJobProcessor(job)
                        .ContinueWith(_ =>
                        {
                            logger.ReleasingJobSlot(job.Id, job.Url);
                            return semaphore.Release();
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

    private async Task InvokeJobProcessor(IJob job)
    {
        try
        {
            logger.StartingJob(job.Id, job.Url);
            await store.StartJobAsync(job.Id);
            
            logger.CallingJobProcessor(job.Id, job.Url);
            await jobProcessor.ProcessAsync(job);

            await store.JobCompletedAsync(job.Id);
            logger.JobCompleted(job.Id, job.Url);
        }
        catch (Exception ex)
        {
            logger.JobFailed(ex, job.Id, job.Url);
            await store.JobFailedAsync(job.Id, ex.Message);
        }
    }
}
