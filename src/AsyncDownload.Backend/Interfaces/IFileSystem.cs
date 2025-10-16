namespace AsyncDownload.Backend.Interfaces;

/// <summary>
/// Provides disk I/O operations.
/// </summary>
internal interface IFileSystem
{
    /// <summary>
    /// Saves the provided content stream to the specified file path asynchronously.
    /// </summary>
    Task SaveToFileAsync(Stream content, Guid jobId, string filePath, CancellationToken ct);
}