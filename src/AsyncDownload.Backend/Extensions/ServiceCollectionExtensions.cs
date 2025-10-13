using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncDownload.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAsyncDownload(this IServiceCollection services)
    {
        return services.AddAsyncDownload(maxConcurrentDownloads: 3);
    }

    public static IServiceCollection AddAsyncDownload(this IServiceCollection services,
        int maxConcurrentDownloads)
    {
        services.AddSingleton<IDownloadManager, DownloadManager>();
        services.AddSingleton<IJobStore, JobStore>();
        services.AddSingleton<JobDispatcher>();
        services.AddTransient<IJobProcessor, DownloadService>();
        services.Configure<DownloadOptions>(options =>
        {
            options.MaxConcurrentDownloads = maxConcurrentDownloads;
        });

        return services;
    }
}