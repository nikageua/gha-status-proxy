namespace GhaStatusProxy.Abstractions;

using Octokit;

public interface IGitHubClientFactory
{

    #region Methods: Public

    IGitHubClient Create(string token);

    #endregion
}
