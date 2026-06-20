using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.API.StaticFiles;

public static class ToolkitStaticFileExtensions
{
    public static WebApplication MapToolkitStaticFiles(
        this WebApplication app,
        string wwwroot,
        ILogger? logger = null)
    {
        if (!Directory.Exists(wwwroot))
            Directory.CreateDirectory(wwwroot);

        if (!File.Exists(Path.Combine(wwwroot, "index.html")))
        {
            logger?.LogWarning(
                "Config UI assets missing at {Wwwroot}. Run dotnet build to generate wwwroot/index.html.",
                wwwroot);
        }

        var fileProvider = new PhysicalFileProvider(wwwroot);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });
        return app;
    }

    public static string ResolveWwwRootPath(string contentRootPath)
    {
        var outputWwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        if (File.Exists(Path.Combine(outputWwwroot, "index.html")))
            return outputWwwroot;

        return Path.Combine(contentRootPath, "wwwroot");
    }
}
