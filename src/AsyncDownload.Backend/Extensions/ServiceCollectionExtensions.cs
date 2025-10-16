using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to add AsyncDownload services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AsyncDownload services to the IServiceCollection with specified max concurrent downloads.
    /// </summary>
    public static IServiceCollection AddAsyncDownload(
        this IServiceCollection services,
        int maxConcurrentDownloads)
    {
        services.AddSingleton<IDownloadManager, DownloadManager>();
        services.AddSingleton<IJobStore, JobStore>();
        services.AddSingleton<JobDispatcher>();
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddTransient<IFileSystem, FileSystem>();
        services.Configure<DownloadOptions>(options =>
        {
            options.MaxConcurrentDownloads = maxConcurrentDownloads;
        });

        return services;
    }
}
