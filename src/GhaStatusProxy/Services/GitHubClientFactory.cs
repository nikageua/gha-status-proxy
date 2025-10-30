using GhaStatusProxy.Abstractions;
using Octokit;

namespace GhaStatusProxy.Services;

public sealed class GitHubClientFactory : IGitHubClientFactory
{

    #region Methods: Public

    public IGitHubClient Create(string token)
    {
        var client = new GitHubClient(new ProductHeaderValue("gha-status-proxy"));
        client.Credentials = new Credentials(token);
        return client;
    }
    
    #endregion
}