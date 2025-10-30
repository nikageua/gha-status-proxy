using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using Octokit;

namespace GhaStatusProxy.Services;

public sealed class GhaService : IGhaService
{

    #region Fields: Private

    private readonly IGitHubClientFactory _factory;
    
    #endregion
    
    #region Methods: Public

    public GhaService(IGitHubClientFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<GhaRunDto>> GetLatestRunsAsync(string token, IEnumerable<string> repos)
    {
        var client = _factory.Create(token);
        var tasks = repos.Select(r => FetchLatestAsync(client, r));
        var results = await Task.WhenAll(tasks);
        return results.Where(x => x is not null).Cast<GhaRunDto>().ToArray();
    }
    
    #endregion
    
    #region Methods: Private

    private static async Task<GhaRunDto?> FetchLatestAsync(IGitHubClient client, string repoFullName)
    {
        var parts = repoFullName.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return null;

        var owner = parts[0];
        var name = parts[1];

        try
        {
            var request = new WorkflowRunsRequest();
            var options = new ApiOptions { PageSize = 1, PageCount = 1, StartPage = 1 };
            var runs = await client.Actions.Workflows.Runs.List(owner, name, request, options);

            if (runs?.WorkflowRuns == null || runs.WorkflowRuns.Count == 0) return new GhaRunDto(
                Repo: repoFullName,
                Status: "empty",
                Conclusion: null,
                Url: $"https://github.com/{repoFullName}/actions",
                UpdatedAt: DateTimeOffset.MinValue,
                Name: null,
                Event: ""
            );

            var run = runs.WorkflowRuns[0];
            return new GhaRunDto(
                Repo: repoFullName,
                Status: run.Status.ToString(),
                Conclusion: run.Conclusion?.ToString(),
                Url: run.HtmlUrl ?? $"https://github.com/{repoFullName}/actions",
                UpdatedAt: run.UpdatedAt,
                Name: run.Name,
                Event: run.Event ?? ""
            );
        }
        catch (NotFoundException)
        {
            return new GhaRunDto(
                Repo: repoFullName,
                Status: "error",
                Conclusion: "not_found",
                Url: $"https://github.com/{repoFullName}/actions",
                UpdatedAt: DateTimeOffset.MinValue,
                Name: null,
                Event: ""
            );
        }
        catch (AuthorizationException)
        {
            return new GhaRunDto(
                Repo: repoFullName,
                Status: "error",
                Conclusion: "unauthorized",
                Url: $"https://github.com/{repoFullName}/actions",
                UpdatedAt: DateTimeOffset.MinValue,
                Name: null,
                Event: ""
            );
        }
        catch (RateLimitExceededException)
        {
            return new GhaRunDto(
                Repo: repoFullName,
                Status: "error",
                Conclusion: "rate_limited",
                Url: $"https://github.com/{repoFullName}/actions",
                UpdatedAt: DateTimeOffset.MinValue,
                Name: null,
                Event: ""
            );
        }
        catch
        {
            return new GhaRunDto(
                Repo: repoFullName,
                Status: "error",
                Conclusion: "unknown_error",
                Url: $"https://github.com/{repoFullName}/actions",
                UpdatedAt: DateTimeOffset.MinValue,
                Name: null,
                Event: ""
            );
        }
    }
    
    #endregion
}