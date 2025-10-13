using Microsoft.Extensions.Logging;

namespace AsyncDownload.Backend.Extensions;

internal static partial class LoggingExtensions
{
    [LoggerMessage(1, LogLevel.Debug, "Starting Job Dispatcher with max {MaxConcurrentJobs} concurrent jobs.")]
    public static partial void StartingJobDispatcher(this ILogger logger, int maxConcurrentJobs);

    [LoggerMessage(2, LogLevel.Debug, "Job Dispatcher producer thread started.")]
    public static partial void JobDispatcherProducerThreadStarted(this ILogger logger);
    
    [LoggerMessage(3, LogLevel.Debug, "Checking job queue for new jobs...")]
    public static partial void CheckJobQueue(this ILogger logger);
    
    [LoggerMessage(4, LogLevel.Debug, "Waiting for a new job...")]
    public static partial void WaitingForJob(this ILogger logger);
    
    [LoggerMessage(5, LogLevel.Debug, "Waiting for available job slot to start downloading from {Url}. Job {JobId}.")]
    public static partial void WaitForAvailableJobSlot(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(6, LogLevel.Debug, "Acquired a job slot for {Url}. Job {JobId}.")]
    public static partial void AcquiredTheJobSlot(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(7, LogLevel.Debug, "Releasing a job slot for {Url}. Job {JobId}.")]
    public static partial void ReleasingJobSlot(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(8, LogLevel.Debug, "Stopping Job Dispatcher.")]
    public static partial void StoppingJobDispatcher(this ILogger logger);

    [LoggerMessage(9, LogLevel.Error, "Job Dispatcher producer thread failed.")]
    public static partial void JobDispatcherProducerThreadFailed(this ILogger logger, Exception exception);

    [LoggerMessage(10, LogLevel.Debug, "Starting downloading from {Url}. Job {JobId}")]
    public static partial void StartingJob(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(11, LogLevel.Debug, "Downloading {Url} completed successfully. Job {JobId}.")]
    public static partial void JobCompleted(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(12, LogLevel.Error, "Downloading {Url} failed. Job {JobId}.")]
    public static partial void JobFailed(this ILogger logger, Exception ex, Guid jobId, string url);

    [LoggerMessage(13, LogLevel.Debug, "Calling job processor for {Url}. Job {JobId}.")]
    public static partial void CallingJobProcessor(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(14, LogLevel.Debug, "Creating HttpClient for {Url}. Job {JobId}.")]
    public static partial void CreateHttpClient(this ILogger logger, Guid jobId, string url);

    [LoggerMessage(15, LogLevel.Debug, "Saving response to file {FilePath}. Job {JobId}.")]
    public static partial void SaveResponseToFile(this ILogger logger, Guid jobId, string filePath);

    [LoggerMessage(16, LogLevel.Debug, "Response successfully written to file {FilePath}. Job {JobId}.")]
    public static partial void ResponseSuccessfullyWrittenToFile(this ILogger logger, Guid jobId, string filePath);
}
