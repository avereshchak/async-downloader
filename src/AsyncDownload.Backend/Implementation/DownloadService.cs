using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

internal class DownloadService : IJobProcessor
{
    private readonly CancellationToken ct;
    private readonly ILogger<DownloadService> logger;

    public DownloadService(IOptions<DownloadOptions> options, ILogger<DownloadService> logger)
    {
        ct = options.Value.AppStoppingToken;
        this.logger = logger;
    }

    public async Task ProcessAsync(IJob job)
    {
        logger.CreateHttpClient(job.Id, job.Url);
        
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(job.Url, ct);
        response.EnsureSuccessStatusCode();

        logger.SaveResponseToFile(job.Id, job.FilePath);
        
        await using var fileStream = new FileStream(
            job.FilePath, 
            FileMode.Create, 
            FileAccess.Write, 
            FileShare.None);

        await response.Content.CopyToAsync(fileStream, ct);

        logger.ResponseSuccessfullyWrittenToFile(job.Id, job.FilePath);
    }
}