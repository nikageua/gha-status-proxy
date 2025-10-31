using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;

namespace GhaStatusProxy.Services;

public sealed class EnvConfigStore : IConfigStore
{
    public Task<AppConfig> LoadAsync()
    {
        var token = Environment.GetEnvironmentVariable("GitHub__Token")
                    ?? Environment.GetEnvironmentVariable("GitHub:Token")
                    ?? string.Empty;

        var repos = new List<string>();
        for (var i = 0; i < 100; i++)
        {
            var v = Environment.GetEnvironmentVariable($"GitHub__Repos__{i}")
                    ?? Environment.GetEnvironmentVariable($"GitHub:Repos:{i}");
            if (string.IsNullOrWhiteSpace(v)) break;
            repos.Add(v);
        }

        return Task.FromResult(new AppConfig(token, repos.ToArray()));
    }

    // Environment is read-only for this store; persist to file fallback instead.
    public Task SaveAsync(AppConfig cfg)
    {
        throw new NotSupportedException("EnvConfigStore is read-only.");
    }
}
