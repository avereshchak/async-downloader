namespace AsyncDownload.Backend.Interfaces;

internal interface IJobProcessor
{
    Task ProcessAsync(IJob job);
}