using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;

namespace GhaStatusProxy.Services;

public sealed class CompositeConfigStore : IConfigStore
{
    private readonly IConfigStore _primary; // Env
    private readonly IConfigStore _fallback; // File

    public CompositeConfigStore(IConfigStore primary, IConfigStore fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public async Task<AppConfig> LoadAsync()
    {
        var cfg = await _primary.LoadAsync();
        if (!string.IsNullOrWhiteSpace(cfg.Token) && cfg.Repos is { Length: > 0 }) return cfg;
        return await _fallback.LoadAsync();
    }

    public Task SaveAsync(AppConfig cfg)
    {
        // Persist to fallback (file) by default.
        return _fallback.SaveAsync(cfg);
    }
}
