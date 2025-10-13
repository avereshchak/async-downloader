using AsyncDownload.Backend.Implementation;
using AsyncDownload.Backend.Interfaces;
using FakeItEasy;

namespace AsyncDownload.Tests;

[TestClass]
public class DownloadManagerTests
{
#pragma warning disable CS8618 
    private DownloadManager sut;
    private IJobStore store;
#pragma warning restore CS8618

    [TestInitialize]
    public void Setup()
    {
        store = A.Fake<IJobStore>();
        sut = new DownloadManager(store);
    }

    [TestMethod]
    public async Task ScheduleDownloadAsync_ValidParameters_JobQueued()
    {
        await sut.ScheduleDownloadAsync("http://localhost/file.html", "file.html");

        A.CallTo(() => store.EnqueueDownloadAsync("http://localhost/file.html", "file.html"))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ScheduleDownloadAsync_InvalidUrl_ThrowsArgumentException()
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
        {
            await sut.ScheduleDownloadAsync("invalid-url", "file.html");
        });
    }

    [TestMethod]
    public async Task ScheduleDownloadAsync_EmptyFilePath_ThrowsArgumentException()
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
        {
            await sut.ScheduleDownloadAsync("http://localhost/file.html", "");
        });
    }

    [TestMethod]
    public async Task GetAllJobsAsync_StoreHasJobs_ReturnsJobs()
    {
        var jobs = new List<IJob>
        {
            A.Fake<IJob>(),
            A.Fake<IJob>(),
        };
        A.CallTo(() => store.GetAllAsync()).Returns(jobs);

        var result = await sut.GetAllJobsAsync();
        
        Assert.AreEqual(2, result.Count());
        CollectionAssert.AreEquivalent(jobs, result.ToList());
    }

    [TestMethod]
    public async Task GetAllJobsAsync_StoreHasNoJobs_ReturnsEmpty()
    {
        A.CallTo(() => store.GetAllAsync()).Returns(Enumerable.Empty<IJob>());
     
        var result = await sut.GetAllJobsAsync();

        Assert.AreEqual(0, result.Count());
    }
}