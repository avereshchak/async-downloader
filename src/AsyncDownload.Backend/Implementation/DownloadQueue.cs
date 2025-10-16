using System.Threading.Channels;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class DownloadQueue : IDownloadQueue
{
    private readonly Channel<IJob> queue;
    private readonly CancellationToken ct;

    public DownloadQueue(IOptions<DownloadOptions> options)
    {
        ct = options.Value.StopToken;
        queue = Channel.CreateUnbounded<IJob>();
    }

    public async Task EnqueueAsync(IJob job)
    {
        await queue.Writer.WriteAsync(job, ct);
    }

    public async Task<IJob> DequeueAsync()
    {
        // Wait until a job is available or cancellation is requested.
        return await queue.Reader.ReadAsync(ct);
    }
}
