using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Backend.Implementation;

/// <summary>
/// A simple service for downloading given URLs to specified file paths.
/// </summary>
internal class DownloadService : IJobProcessor
{
    private readonly CancellationToken ct;
    private readonly ILogger<DownloadService> logger;

    public DownloadService(
        IOptions<DownloadOptions> options, 
        ILogger<DownloadService> logger)
    {
        ct = options.Value.StopToken;
        this.logger = logger;
    }

    public async Task ProcessAsync(IJob job)
    {
        logger.CreateHttpClient(job.Id, job.Url);
        
        // Theoretically, this service can be split into separate services.
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
