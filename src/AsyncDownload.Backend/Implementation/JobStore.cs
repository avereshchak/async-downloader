using System.Collections.Concurrent;
using System.Threading.Channels;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class JobStore : IJobStore
{
    private readonly ConcurrentDictionary<Guid, Job> jobs;
    private readonly Channel<IJob> jobQueue;
    private readonly CancellationToken ct;

    public JobStore(IOptions<DownloadOptions> options)
    {
        jobs = new ConcurrentDictionary<Guid, Job>();
        jobQueue = Channel.CreateUnbounded<IJob>();
        ct = options.Value.AppStoppingToken;
    }

    public Task<IEnumerable<IJob>> GetAllAsync()
    {
        return Task.FromResult(jobs.Values.Cast<IJob>());
    }

    public async Task<IJob> DequeueAsync()
    {
        return await jobQueue.Reader.ReadAsync(ct);
    }

    public async Task EnqueueDownloadAsync(string url, string filePath)
    {
        // Register a new job.
        var job = new Job(url, filePath)
        {
            Status = DownloadJobStatus.Queued
        };

        jobs[job.Id] = job;

        await jobQueue.Writer.WriteAsync(job, ct);
    }

    public Task<bool> StartJobAsync(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out var job))
        {
            // Not found, it might have been deleted.
            return Task.FromResult(false);
        }

        if (job.Status != DownloadJobStatus.Queued)
        {
            // Unexpected status. It might have been already started, completed or canceled.
            return Task.FromResult(false);
        }

        job.Status = DownloadJobStatus.InProgress;
        job.Timestamp = DateTimeOffset.UtcNow;

        return Task.FromResult(true);
    }

    public Task JobCompletedAsync(Guid jobId)
    {
        if (jobs.TryGetValue(jobId, out var job))
        {
            job.Status = DownloadJobStatus.DownloadedSuccessfully;
            job.Timestamp = DateTimeOffset.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task JobFailedAsync(Guid jobId, string statusMessage)
    {
        if (jobs.TryGetValue(jobId, out var job))
        {
            job.Status = DownloadJobStatus.DownloadFailed;
            job.StatusMessage = statusMessage;
            job.Timestamp = DateTimeOffset.UtcNow;
        }
        return Task.CompletedTask;
    }
}