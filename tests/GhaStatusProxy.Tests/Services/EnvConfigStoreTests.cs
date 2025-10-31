using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GhaStatusProxy.Services;
using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public sealed class EnvConfigStoreTests : IDisposable
{
    private static readonly string[] KeysToClean =
        Enumerable.Empty<string>()
        .Concat(new[] { "GitHub__Token", "GitHub:Token" })
        .Concat(Enumerable.Range(0, 10).Select(i => $"GitHub__Repos__{i}"))
        .Concat(Enumerable.Range(0, 10).Select(i => $"GitHub:Repos:{i}"))
        .ToArray();

    public EnvConfigStoreTests() => ClearEnv();
    public void Dispose() => ClearEnv();

    private static void ClearEnv()
    {
        foreach (var k in KeysToClean)
            Environment.SetEnvironmentVariable(k, null);
    }

    [Fact]
    public async Task LoadAsync_ReturnsEmptyConfig_WhenNoEnv()
    {
        var store = new EnvConfigStore();

        var cfg = await store.LoadAsync();

        cfg.Should().NotBeNull();
        cfg.Token.Should().BeNullOrEmpty();
        cfg.Repos.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_Prefers_DoubleUnderscore_Over_Colon()
    {
        Environment.SetEnvironmentVariable("GitHub:Token", "colon");
        Environment.SetEnvironmentVariable("GitHub__Token", "dd");

        var store = new EnvConfigStore();
        var cfg = await store.LoadAsync();

        cfg.Token.Should().Be("dd");
    }

    [Fact]
    public async Task LoadAsync_ReadsRepos_Sequentially_StopsOnGap()
    {
        Environment.SetEnvironmentVariable("GitHub__Token", "pat");
        Environment.SetEnvironmentVariable("GitHub__Repos__0", "a/b");
        Environment.SetEnvironmentVariable("GitHub__Repos__1", "c/d");
        Environment.SetEnvironmentVariable("GitHub__Repos__3", "e/f");

        var store = new EnvConfigStore();
        var cfg = await store.LoadAsync();

        cfg.Repos.Should().ContainInOrder("a/b", "c/d");
        cfg.Repos.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveAsync_Throws_NotSupported()
    {
        var store = new EnvConfigStore();
        var cfg = new AppConfig("", Array.Empty<string>());
        Func<Task> act = async () => await store.SaveAsync(cfg);
        await act.Should().ThrowAsync<NotSupportedException>();
    }
}