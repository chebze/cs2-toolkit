using CS2Toolkit.API.Abstractions;
using CS2Toolkit.API.Json;
using CS2Toolkit.Configuration;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CS2Toolkit.API.Endpoints;

public static class WebApplicationExtensions
{
    public static WebApplication MapToolkitApi(this WebApplication app)
    {
        var json = ToolkitJsonSerializerOptions.Web;

        app.MapGet("/api/dashboard", (IDashboardInfoProvider dashboard) =>
            Results.Json(dashboard.GetDashboardInfo(), json));

        app.MapGet("/api/configs", (IConfigurationStore store) =>
            Results.Json(store.GetStore(), json));

        app.MapGet("/api/configs/{id}", (string id, IConfigurationStore store) =>
        {
            var profile = store.GetProfile(id);
            return profile is null ? Results.NotFound() : Results.Json(profile, json);
        });

        app.MapPost("/api/configs", (CreateProfileRequest request, IConfigurationStore store) =>
        {
            var profile = store.CreateProfile(request.Name);
            return Results.Json(profile, json);
        });

        app.MapPut("/api/configs/{id}", (string id, ConfigProfile profile, IConfigurationStore store, IActiveProfileSwitcher profileSwitcher) =>
        {
            if (id != profile.Id)
                return Results.BadRequest("Profile id mismatch.");

            var updated = store.UpdateProfile(profile);
            if (store.GetActiveProfile().Id == id)
                profileSwitcher.ApplyActiveProfileToggles(updated.Settings);

            return Results.Json(updated, json);
        });

        app.MapDelete("/api/configs/{id}", (string id, IConfigurationStore store) =>
        {
            store.DeleteProfile(id);
            return Results.NoContent();
        });

        app.MapPost("/api/configs/{id}/activate", (string id, IActiveProfileSwitcher profileSwitcher) =>
        {
            profileSwitcher.SwitchTo(id);
            return Results.Ok();
        });

        app.MapPost("/api/configs/{id}/default", (string id, IConfigurationStore store) =>
        {
            store.SetDefaultProfile(id);
            return Results.Ok();
        });

        app.MapGet("/api/configs/{id}/export", (string id, IConfigurationStore store) =>
        {
            try
            {
                return Results.Text(store.ExportProfile(id), "application/json");
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        app.MapPost("/api/configs/import", async (HttpRequest request, IConfigurationStore store) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            var name = request.Query.TryGetValue("name", out var nameValues)
                ? nameValues.FirstOrDefault()
                : null;
            var profile = store.ImportProfile(body, name);
            return Results.Json(profile, json);
        });

        app.MapGet("/api/keybinds", (IConfigurationStore store) =>
            Results.Json(store.GetStore().Keybinds, json));

        app.MapPut("/api/keybinds", (GlobalKeybinds keybinds, IConfigurationStore store) =>
        {
            store.UpdateKeybinds(keybinds);
            return Results.Ok();
        });

        app.MapGet("/api/weapons", () => Results.Json(WeaponCatalog.All, json));

        app.MapGet("/api/radar/snapshot", (IRadarStreamSource radar) =>
            Results.Json(radar.GetSnapshot(), json));

        app.MapGet("/api/radar/stream", async (
            HttpContext context,
            IRadarStreamSource radar,
            CancellationToken cancellationToken) =>
        {
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.ContentType = "text/event-stream";

            var lastVersion = -1L;
            while (!cancellationToken.IsCancellationRequested)
            {
                var version = radar.Version;
                if (version != lastVersion)
                {
                    lastVersion = version;
                    await context.Response.WriteAsync(
                        $"data: {radar.GetSnapshotJson()}\n\n",
                        cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }

                await Task.Delay(100, cancellationToken);
            }
        });

        return app;
    }
}
