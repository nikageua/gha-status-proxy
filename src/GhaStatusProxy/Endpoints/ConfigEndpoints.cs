using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;

namespace GhaStatusProxy.Endpoints;

public static class ConfigEndpoints
{

    #region Methods: Public

    public static IEndpointRouteBuilder MapConfigEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/config", async (IConfigStore store) =>
        {
            var cfg = await store.LoadAsync();
            return Results.Ok(new { repos = cfg.Repos });
        });

        app.MapPost("/api/config", async (IConfigStore store, UpdateConfigRequest req) =>
        {
            if (req is null) return Results.BadRequest(new { error = "invalid_json" });
            if (string.IsNullOrWhiteSpace(req.Token)) return Results.BadRequest(new { error = "token_required" });
            if (req.Repos is null) return Results.BadRequest(new { error = "repos_required" });

            var repos = req.Repos
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            await store.SaveAsync(new AppConfig(req.Token.Trim(), repos));
            return Results.Ok(new { saved = true, repos = repos.Length });
        });

        return app;
    }
    
    #endregion
}