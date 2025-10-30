using FluentAssertions;
using GhaStatusProxy.Services;
using Octokit;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public class GitHubClientFactoryTests
{
    [Fact]
    public void Create_ReturnsClient_WithCredentials()
    {
        var factory = new GitHubClientFactory();
        var client = factory.Create("my-token");

        client.Should().NotBeNull();
        var typed = client as GitHubClient;
        typed.Should().NotBeNull();
        typed!.Credentials.Should().NotBeNull();
    }
}
