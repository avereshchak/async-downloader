using System.Threading.Channels;
using AsyncDownload.Backend.Interfaces;

namespace AsyncDownload.Backend.Implementation;

internal class DownloadQueue : IDownloadQueue
{
    private readonly Channel<IJob> queue;

    public DownloadQueue()
    {
        queue = Channel.CreateUnbounded<IJob>();
    }

    public async Task EnqueueAsync(IJob job, CancellationToken ct)
    {
        await queue.Writer.WriteAsync(job, ct);
    }

    public async Task<IJob> DequeueAsync(CancellationToken ct)
    {
        // Wait until a job is available or cancellation is requested.
        return await queue.Reader.ReadAsync(ct);
    }
}
