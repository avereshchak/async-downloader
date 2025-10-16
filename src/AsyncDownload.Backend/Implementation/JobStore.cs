using System.Collections.Concurrent;
using AsyncDownload.Backend.Interfaces;

namespace AsyncDownload.Backend.Implementation;

/// <summary>
/// In-memory job store and queue. Not production ready, sorry.
/// </summary>
internal class JobStore : IJobStore
{
    private readonly ConcurrentDictionary<Guid, Job> jobs;

    public JobStore()
    {
        jobs = new ConcurrentDictionary<Guid, Job>();
    }

    public Task<IEnumerable<IJob>> GetAllAsync()
    {
        return Task.FromResult(jobs.Values.Cast<IJob>());
    }

    public Task<IJob> AddDownloadJobAsync(string url, string filePath)
    {
        // Register a new job.
        var job = new Job(url, filePath)
        {
            Status = DownloadStatus.Queued
        };

        jobs[job.Id] = job;
        return Task.FromResult(job as IJob);
    }

    public Task<bool> MarkAsStarted(Guid jobId)
    {
        if (!jobs.TryGetValue(jobId, out var job))
        {
            // Not found, it might have been deleted.
            return Task.FromResult(false);
        }

        if (job.Status != DownloadStatus.Queued)
        {
            // Unexpected status. It might have been already started, completed or canceled.
            return Task.FromResult(false);
        }

        job.Status = DownloadStatus.InProgress;
        job.Timestamp = DateTimeOffset.UtcNow;

        return Task.FromResult(true);
    }

    public Task MarkAsCompletedAsync(Guid jobId)
    {
        if (jobs.TryGetValue(jobId, out var job))
        {
            job.Status = DownloadStatus.DownloadedSuccessfully;
            job.Timestamp = DateTimeOffset.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task MarkAsFailed(Guid jobId, string statusMessage)
    {
        if (jobs.TryGetValue(jobId, out var job))
        {
            job.Status = DownloadStatus.DownloadFailed;
            job.StatusMessage = statusMessage;
            job.Timestamp = DateTimeOffset.UtcNow;
        }
        return Task.CompletedTask;
    }
}
