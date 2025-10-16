using Microsoft.AspNetCore.Builder;

namespace AsyncDownload.Tests;

/// <summary>
/// A simple web server for integration testing purposes.
/// </summary>
internal class MyWebServer : IAsyncDisposable
{
    private readonly WebApplication app;

    public MyWebServer()
    {
        var builder = WebApplication.CreateBuilder();
        app = builder.Build();
        app.MapGet("/download/{id}", (string id) => $"You requested download for ID: {id}");
    }

    public async Task StartAsync()
    {
        await app.StartAsync();
    }

    public ValueTask DisposeAsync()
    {
        return app.DisposeAsync();
    }
}