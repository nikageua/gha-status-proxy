using System.Collections;
using System.Text.RegularExpressions;
using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using Microsoft.Extensions.Logging;

namespace GhaStatusProxy.Services;

public sealed class EnvConfigStore : IConfigStore
{
    private readonly ILogger<EnvConfigStore> _logger;
    private static readonly Regex RepoKey = new(@"^GitHub(__|:)Repos(__|:)(\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public EnvConfigStore(ILogger<EnvConfigStore> logger)
    {
        _logger = logger;
    }

    public Task<AppConfig> LoadAsync()
    {
        var token = Environment.GetEnvironmentVariable("GitHub__Token")
                 ?? Environment.GetEnvironmentVariable("GitHub:Token")
                 ?? string.Empty;

        _logger.LogInformation("EnvConfigStore: GitHub__Token length = {Length}", token.Length);

        var env = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
        var repos = new List<(int idx, string repo)>(capacity: 8);

        foreach (DictionaryEntry de in env)
        {
            var key = de.Key?.ToString() ?? "";
            var val = de.Value?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(val)) continue;

            var m = RepoKey.Match(key);
            if (!m.Success) continue;
            if (!int.TryParse(m.Groups[3].Value, out var idx)) continue;

            repos.Add((idx, val.Trim()));
            _logger.LogInformation("EnvConfigStore: Found repo var {Key} = {Value}", key, val);
        }

        var ordered = repos
            .OrderBy(t => t.idx)
            .Select(t => t.repo)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        _logger.LogInformation("EnvConfigStore: Total repos found = {Count}", ordered.Length);

        return Task.FromResult(new AppConfig(token, ordered));
    }

    public Task SaveAsync(AppConfig config)
        => throw new NotSupportedException("Saving env-backed config is not supported.");
}
