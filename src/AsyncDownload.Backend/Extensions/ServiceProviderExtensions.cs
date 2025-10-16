using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend.Extensions;

/// <summary>
/// Extension methods for activating the async download service.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Activates the async download service. This is required to start the background processing.
    /// </summary>
    public static IDownloadManager ActivateAsyncDownload(this IServiceProvider provider)
    {
        // Activating the JobDispatcher will start the background processing.
        // It would be better to use IHostedService, but it requires an explicit dependency
        // to Microsoft.Extensions.Hosting, which I wanted to avoid.
        _ = provider.GetRequiredService<JobDispatcher>();
        
        return provider.GetRequiredService<IDownloadManager>();
    }
}
