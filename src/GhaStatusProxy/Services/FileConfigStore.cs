using System.Text.Json;
using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using GhaStatusProxy.Options;
using Microsoft.Extensions.Options;

namespace GhaStatusProxy.Services;

public sealed class FileConfigStore(IOptions<ConfigStoreOptions> options) : IConfigStore
{

    #region Fields: Private

    private readonly string _path = options.Value.Path;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };
    
    #endregion
    
    #region Methods: Public

    public async Task<AppConfig> LoadAsync()
    {
        await _gate.WaitAsync();
        try {
            if (!File.Exists(_path)) return new AppConfig("", Array.Empty<string>());
            using var stream = File.OpenRead(_path);
            var cfg = await JsonSerializer.DeserializeAsync<AppConfig>(stream, JsonOpts);
            return cfg ?? new AppConfig("", Array.Empty<string>());
        }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(AppConfig cfg)
    {
        await _gate.WaitAsync();
        try {
            var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            await using var stream = File.Create(_path);
            await JsonSerializer.SerializeAsync(stream, cfg, JsonOpts);
            try { File.SetAttributes(_path, File.GetAttributes(_path) | FileAttributes.Hidden); } catch { }
        }
        finally { _gate.Release(); }
    }
    
    #endregion
}