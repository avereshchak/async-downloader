namespace AsyncDownload.Backend.Interfaces;

public interface IJob
{
    public Guid Id { get; }
    public string Url { get; }
    public string FilePath { get; }
    public DownloadJobStatus Status { get; }
    public string? StatusMessage { get; }
}