using AsyncDownload.Backend;

namespace AsyncDownload.Tests;

[TestClass]
public class IntegrationTests
{
    private readonly string downloadsFolder  = Path.Combine(Path.GetTempPath(), "AsyncDownloadTests");

    [TestMethod]
    public async Task DownloadManyUrls_AllDownloaded()
    {
        // This test schedules downloading 100 files from a local web server
        // and verifies that all files are downloaded successfully.
        await using var webServer = new MyWebServer();
        await webServer.StartAsync();

        // Allow 10 concurrent downloads.
        using var downloadClient = new DownloadClient(10);

        // Schedule 100 downloads.
        for (var i = 0; i < 100; i++)
        {
            var link = $"http://localhost:5000/download/{i}";
            var filePath = Path.Combine(downloadsFolder, $"{i}.txt");

            await downloadClient.ScheduleDownloadAsync(link, filePath);
        }

        // Let the download manager do its work.
        await downloadClient.WaitForCompletionAsync(TimeSpan.FromSeconds(10));

        // ASSERT
        // All jobs should be completed successfully.
        var jobs = await downloadClient.GetAllJobsAsync();
        foreach (var job in jobs)
        {
            Assert.AreEqual(DownloadStatus.DownloadedSuccessfully, job.Status, job.StatusMessage);
        }

        // There should be 100 files in the downloads folder.
        var downloadedFiles = Directory.EnumerateFiles(downloadsFolder);
        Assert.AreEqual(100, downloadedFiles.Count());

        // TODO: check file contents
    }

    [TestInitialize]
    public void Setup()
    {
        if (!Directory.Exists(downloadsFolder))
        {
            Directory.CreateDirectory(downloadsFolder);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (!Directory.Exists(downloadsFolder))
        {
            return;
        }

        var files = Directory.EnumerateFiles(downloadsFolder);
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
}
