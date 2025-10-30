using System.Text.Json;
using FluentAssertions;
using GhaStatusProxy.Models;
using GhaStatusProxy.Options;
using GhaStatusProxy.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public class FileConfigStoreTests
{
    [Fact]
    public async Task LoadAsync_WhenFileMissing_ReturnsDefaults()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var store = new FileConfigStore(new OptionsWrapper<ConfigStoreOptions>(new ConfigStoreOptions { Path = tempPath }));

        var cfg = await store.LoadAsync();

        cfg.Token.Should().BeEmpty();
        cfg.Repos.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsConfig()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var store = new FileConfigStore(new OptionsWrapper<ConfigStoreOptions>(new ConfigStoreOptions { Path = tempPath }));
        var toSave = new AppConfig("tok", new[] { "a/b", "c/d" });

        await store.SaveAsync(toSave);
        var loaded = await store.LoadAsync();

        loaded.Should().BeEquivalentTo(toSave);
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectory_WhenMissing()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempPath = Path.Combine(dir, "config.json");
        var store = new FileConfigStore(new OptionsWrapper<ConfigStoreOptions>(new ConfigStoreOptions { Path = tempPath }));

        var toSave = new AppConfig("tok", Array.Empty<string>());
        await store.SaveAsync(toSave);

        File.Exists(tempPath).Should().BeTrue();

        // Ensure JSON is valid
        var json = await File.ReadAllTextAsync(tempPath);
        var parsed = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        parsed.Should().NotBeNull();
    }
}

