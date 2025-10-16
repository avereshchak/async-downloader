using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace AsyncDownload.Backend.Implementation;

/// <summary>
/// A simple service for downloading given URLs to specified file paths.
/// </summary>
internal class DownloadService : IDownloadService
{
    private readonly ILogger<DownloadService> logger;

    public DownloadService(ILogger<DownloadService> logger)
    {
        this.logger = logger;
    }

    public async Task<Stream> DownloadAsync(Guid jobId, string url, CancellationToken ct)
    {
        logger.CreateHttpClient(jobId, url);
        using var httpClient = new HttpClient();

        // TODO: retry policy
        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(ct);
    }
}