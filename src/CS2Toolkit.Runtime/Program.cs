using CS2Toolkit.API;
using CS2Toolkit.Configuration;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.WinForms;
using CS2Toolkit.Game;
using CS2Toolkit.Input;
using CS2Toolkit.Models;
using CS2Toolkit.Runtime;
using CS2Toolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Runtime;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<CS2Toolkit.Configuration.Abstractions.ToolkitHostSettings>(
                    context.Configuration.GetSection(CS2Toolkit.Configuration.Abstractions.ToolkitHostSettings.SectionName));

                services
                    .AddToolkitModels()
                    .AddToolkitConfiguration()
                    .AddToolkitInput()
                    .AddToolkitGame()
                    .AddDrawingWinForms()
                    .AddToolkitServices()
                    .AddToolkitApi()
                    .AddRuntimeOrchestration();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        await host.RunAsync();
    }
}
