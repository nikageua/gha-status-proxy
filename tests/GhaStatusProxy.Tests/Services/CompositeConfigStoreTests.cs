using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using GhaStatusProxy.Services;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public sealed class CompositeConfigStoreTests
{
    private sealed class TestStore : IConfigStore
    {
        private readonly AppConfig _data;
        public int SaveCalls { get; private set; }

        public TestStore(AppConfig data) => _data = data; 

        public Task<AppConfig> LoadAsync() => Task.FromResult(_data);

        public Task SaveAsync(AppConfig config)
        {
            SaveCalls++;
            return Task.CompletedTask;
        }
    }

    private static AppConfig Cfg(string token, params string[] repos)
        => new AppConfig(token, repos);

    [Fact]
    public async Task LoadAsync_ReturnsPrimary_WhenTokenAndReposPresent()
    {
        var primary  = new TestStore(Cfg("t1", "a/b", "c/d"));
        var fallback = new TestStore(Cfg("t2", "x/y"));

        var composite = new CompositeConfigStore(primary, fallback);
        var cfg = await composite.LoadAsync();

        cfg.Token.Should().Be("t1");
        cfg.Repos.Should().BeEquivalentTo(new[] { "a/b", "c/d" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LoadAsync_FallsBack_WhenPrimaryTokenMissing()
    {
        var primary  = new TestStore(Cfg("", "a/b"));
        var fallback = new TestStore(Cfg("t2", "x/y"));

        var composite = new CompositeConfigStore(primary, fallback);
        var cfg = await composite.LoadAsync();

        cfg.Token.Should().Be("t2");
        cfg.Repos.Should().BeEquivalentTo(new[] { "x/y" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LoadAsync_FallsBack_WhenPrimaryReposEmpty()
    {
        var primary  = new TestStore(Cfg("t1"));
        var fallback = new TestStore(Cfg("t2", "x/y"));

        var composite = new CompositeConfigStore(primary, fallback);
        var cfg = await composite.LoadAsync();

        cfg.Token.Should().Be("t2");
        cfg.Repos.Should().BeEquivalentTo(new[] { "x/y" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task SaveAsync_Delegates_To_Fallback_Store()
    {
        var primary  = new TestStore(Cfg("t1", "a/b"));
        var fallback = new TestStore(Cfg("t2", "x/y"));

        var composite = new CompositeConfigStore(primary, fallback);

        var newCfg = Cfg("t-new", "n/m");
        await composite.SaveAsync(newCfg);

        primary.SaveCalls.Should().Be(0);
        fallback.SaveCalls.Should().Be(1);
    }
}
