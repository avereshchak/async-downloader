using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AsyncDownload.Backend.Extensions;
using Microsoft.Extensions.Configuration;

namespace AsyncDownload.Console;

internal class Program
{
    static async Task Main(string[] args)
    {
        PrintUsage();

        // Configure host.
        var host = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                // Add the AsyncDownload services to the DI container.
                // Tell it to perform a maximum of 3 concurrent downloads.
                services.AddAsyncDownload(maxConcurrentDownloads: 3);
              
                services.AddLogging(logging =>
                {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddSimpleConsole();
                });
            })
            .UseConsoleLifetime()
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .Build();

        // Activate the download manager.
        var downloadManager = host.Services.ActivateAsyncDownload();

        // Read the input.json and schedule downloads.
        foreach (var request in await InputReader.ReadLinksFromJson())
        {
            await downloadManager.ScheduleDownloadAsync(request.Url, request.Output);
        }

        await host.RunAsync();

        // User stopped the app (Ctrl+C). Print the final status of all downloads.
        var jobs = await downloadManager.GetAllJobsAsync();
        foreach (var job in jobs)
        {
            System.Console.WriteLine($"Status of {job.Url} is {job.Status}.");
        }
    }

    private static void PrintUsage()
    {
        System.Console.WriteLine("This is the Asynchronous Download demo host. It reads the links to be downloaded" +
                                 " from the 'input.json' and saves them to the file system.");
        System.Console.WriteLine("Watch the console logs for download progress.");
        System.Console.WriteLine();
        System.Console.WriteLine("To stop the application, press Ctrl+C. It will report the final download status.");
        System.Console.WriteLine("====================================");
        System.Console.WriteLine();
    }
}
