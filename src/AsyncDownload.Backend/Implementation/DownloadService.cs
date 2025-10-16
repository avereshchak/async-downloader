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
    private readonly IHttpClientFactory httpClientFactory;

    public DownloadService(ILogger<DownloadService> logger, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<Stream> DownloadAsync(Guid jobId, string url, CancellationToken ct)
    {
        logger.CreateHttpClient(jobId, url);
        var httpClient = httpClientFactory.CreateClient();
        
        // TODO: retry policy using Microsoft.Extensions.Http.Polly?
        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(ct);
    }
}
