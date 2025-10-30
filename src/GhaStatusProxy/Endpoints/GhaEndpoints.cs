using GhaStatusProxy.Abstractions;

namespace GhaStatusProxy.Endpoints;

public static class GhaEndpoints
{

    #region Methods: Public

    public static IEndpointRouteBuilder MapGhaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/gha/latest", async (IConfigStore store, IGhaService svc) =>
        {
            var cfg = await store.LoadAsync();
            if (string.IsNullOrWhiteSpace(cfg.Token))
                return Results.BadRequest(new { error = "GitHub token not configured" });

            if (cfg.Repos.Length == 0)
                return Results.Ok(new { results = Array.Empty<object>() });

            var results = await svc.GetLatestRunsAsync(cfg.Token, cfg.Repos);
            return Results.Ok(new { results });
        });

        return app;
    }
    
    #endregion
}

