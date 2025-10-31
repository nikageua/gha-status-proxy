using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GhaStatusProxy.Models;
using GhaStatusProxy.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public sealed class EnvConfigStoreTests : IDisposable
{
    private readonly Mock<ILogger<EnvConfigStore>> _logger = new();

    public EnvConfigStoreTests() => ClearEnv();
    public void Dispose() => ClearEnv();

    private static void ClearEnv()
    {
        foreach (var key in Environment.GetEnvironmentVariables().Keys.Cast<string>()
                     .Where(k => k.StartsWith("GitHub", StringComparison.OrdinalIgnoreCase)))
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    [Fact]
    public async Task LoadAsync_WhenNoVariables_ReturnsEmptyAndLogs()
    {
        var store = new EnvConfigStore(_logger.Object);

        var cfg = await store.LoadAsync();

        cfg.Token.Should().BeEmpty();
        cfg.Repos.Should().BeEmpty();

        _logger.Verify(l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Token")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadAsync_ReadsTokenAndReposAndLogsFound()
    {
        Environment.SetEnvironmentVariable("GitHub__Token", "tok123");
        Environment.SetEnvironmentVariable("GitHub__Repos__0", "a/b");
        Environment.SetEnvironmentVariable("GitHub__Repos__1", "c/d");

        var store = new EnvConfigStore(_logger.Object);
        var cfg = await store.LoadAsync();

        cfg.Token.Should().Be("tok123");
        cfg.Repos.Should().ContainInOrder("a/b", "c/d");

        _logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found repo var")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2)); // two repos
    }

    [Fact]
    public async Task SaveAsync_Throws_NotSupported()
    {
        var store = new EnvConfigStore(_logger.Object);
        var act = async () => await store.SaveAsync(new AppConfig("t", Array.Empty<string>()));

        await act.Should().ThrowAsync<NotSupportedException>();
    }
}
