using System.Text.Json.Serialization;
using System.Text.Json;

namespace AsyncDownload.Console;

/// <summary>
/// A helper for reading links to be downloaded from input.json.
/// </summary>
internal class InputReader
{
    private const string InputFilePath = "input.json";

    public record DownloadRequest(string Url, string Output);

    public static async Task<IEnumerable<DownloadRequest>> ReadLinksFromJson()
    {
        if (!File.Exists(InputFilePath))
        {
            // Fail fast if input.json is missing or malformed.
            throw new FileNotFoundException($"{InputFilePath} not found.");
        }

        try
        {
            var json = await File.ReadAllTextAsync(InputFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var result = JsonSerializer.Deserialize<List<DownloadRequest>>(json, options);
            if (result == null)
            {
                throw new ApplicationException($"The {InputFilePath} content is not an expected JSON.");
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"The {InputFilePath} content is not an expected JSON.", ex);
        }
    }
}