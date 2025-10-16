using AsyncDownload.Backend.Extensions;
using AsyncDownload.Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace AsyncDownload.Backend.Implementation;

internal class FileSystem : IFileSystem
{
    private readonly ILogger<FileSystem> logger;

    public FileSystem(ILogger<FileSystem> logger)
    {
        this.logger = logger;
    }

    public async Task SaveToFileAsync(
        Stream content, 
        Guid jobId, 
        string filePath, 
        CancellationToken ct)
    {
        logger.SaveResponseToFile(jobId, filePath);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await content.CopyToAsync(fileStream, ct).ConfigureAwait(false);

        logger.ResponseSuccessfullyWrittenToFile(jobId, filePath);
    }
}