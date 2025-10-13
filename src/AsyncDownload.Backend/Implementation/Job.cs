using AsyncDownload.Backend.Interfaces;

namespace AsyncDownload.Backend.Implementation;

internal class Job : IJob
{
    public Guid Id { get; }
    public DateTimeOffset Timestamp { get; set; }
    public string Url { get; }
    public string FilePath { get; }
    public DownloadJobStatus Status { get; set; }
    public string? StatusMessage { get; set; }

    public Job(string url, string filePath)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        Url = url;
        FilePath = filePath;
    }
}