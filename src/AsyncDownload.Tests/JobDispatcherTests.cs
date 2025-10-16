using AsyncDownload.Backend;
using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AsyncDownload.Tests;

[TestClass]
public sealed class JobDispatcherTests
{
#pragma warning disable CS8618
    private CancellationTokenSource cts;
    private IJobStore store;
    private ILogger<JobDispatcher> logger;
    private IDownloadService downloadService;
    private IFileSystem fileSystem;
    private IDownloadQueue queue;
#pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        store = A.Fake<IJobStore>();
        logger = A.Fake<ILogger<JobDispatcher>>();
        downloadService = A.Fake<IDownloadService>();
        fileSystem = A.Fake<IFileSystem>();
        queue = A.Fake<IDownloadQueue>();

        cts = new CancellationTokenSource();
    }

    [TestCleanup]
    public void Cleanup()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private JobDispatcher CreateSut(int maxConcurrentDownloads)
    {
        var options = A.Fake<IOptions<DownloadOptions>>();
        A.CallTo(() => options.Value).Returns(new DownloadOptions
        {
            MaxConcurrentDownloads = maxConcurrentDownloads,
        });
        return new JobDispatcher(options, store, logger, downloadService, fileSystem, queue);
    }

    [TestMethod]
    public void Ctor_ZeroMaxConcurrentJobs_Exception()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => CreateSut(0));
    }

    [TestMethod]
    public void Ctor_NegativeMaxConcurrentJobs_Exception()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => CreateSut(-1));
    }

    [TestMethod]
    public void Ctor_ValidMaxConcurrentJobs_Success()
    {
        var dispatcher = _ = CreateSut(3);

        Assert.IsNotNull(dispatcher);
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(100)]
    public async Task Concurrency_NeverExceedsLimit(int concurrentJobsLimit)
    {
        // ARRANGE
        // Run a large number of jobs and ensure the number of simultaneous jobs
        // never exceeds maxConcurrentJobs.
        var totalInvocations = 0;
        var concurrentInvocations = 0;
        var maxConcurrentInvocations = 0;

        var syncObject = new object();
        
        var job = A.Fake<IJob>();

        A.CallTo(() => queue.DequeueAsync(A<CancellationToken>._)).Returns(job);
        A.CallTo(() => downloadService.DownloadAsync(job.Id, job.Url, A<CancellationToken>.Ignored))
            .Invokes(() =>
            {
                Interlocked.Increment(ref totalInvocations);

                var current = Interlocked.Increment(ref concurrentInvocations);
                lock (syncObject)
                {
                    if (current > maxConcurrentInvocations)
                    {
                        maxConcurrentInvocations = current;
                    }
                }
            })
            .ReturnsLazily(async (Guid _, string _, CancellationToken _) =>
            {
                await Task.Delay(5, cts.Token);

                Interlocked.Decrement(ref concurrentInvocations);

                return A.Fake<Stream>();
            });

        // ACT
        _ = CreateSut(concurrentJobsLimit);

        // Wait until a large number of jobs is processed (each concurrent task should
        // run at least 20 times).
        while (totalInvocations < 50 * concurrentJobsLimit)
        {
            await Task.Delay(5);
        }
        await cts.CancelAsync();

        // ASSERT
        Assert.IsTrue(maxConcurrentInvocations <= concurrentJobsLimit,
            $"Max active jobs {maxConcurrentInvocations} exceeded max concurrent jobs {concurrentJobsLimit}.");
    }
}