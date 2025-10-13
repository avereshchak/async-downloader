using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend.Extensions;

public static class ServiceProviderExtensions
{
    public static IDownloadManager ActivateAsyncDownload(this IServiceProvider provider)
    {
        // Activating the JobDispatcher will start the background processing.
        _ = provider.GetRequiredService<JobDispatcher>();
        
        return provider.GetRequiredService<IDownloadManager>();
    }
}