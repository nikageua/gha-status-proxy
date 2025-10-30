using FluentAssertions;
using GhaStatusProxy.Abstractions;
using GhaStatusProxy.Models;
using GhaStatusProxy.Services;
using Moq;
using Octokit;
using Xunit;

namespace GhaStatusProxy.Tests.Services;

public class GhaServiceTests
{
    [Fact]
    public async Task GetLatestRunsAsync_ReturnsLatestSingleRun()
    {
        var run = new WorkflowRun(
            id: 1,
            name: "build",
            nodeId: "nid",
            checkSuiteId: 1,
            checkSuiteNodeId: "csnid",
            headBranch: "main",
            headSha: "sha",
            path: ".github/workflows/build.yml",
            runNumber: 1,
            @event: "push",
            displayTitle: "Build",
            status: WorkflowRunStatus.Completed,
            conclusion: WorkflowRunConclusion.Success,
            workflowId: 1,
            url: "api-url",
            htmlUrl: "https://github.com/owner/repo/actions/runs/1",
            pullRequests: Array.Empty<PullRequest>(),
            createdAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            updatedAt: DateTimeOffset.UtcNow,
            actor: new User(),
            runAttempt: 1,
            referencedWorkflows: Array.Empty<WorkflowReference>(),
            runStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            triggeringActor: new User(),
            jobsUrl: "",
            logsUrl: "",
            checkSuiteUrl: "",
            artifactsUrl: "",
            cancelUrl: "",
            rerunUrl: "",
            previousAttemptUrl: "",
            workflowUrl: "",
            headCommit: new Commit(),
            repository: new Repository(),
            headRepository: new Repository(),
            headRepositoryId: 1
        );

        var response = new WorkflowRunsResponse(1, new[] { run });

        var runsClient = new Mock<IActionsWorkflowRunsClient>();
        runsClient.Setup(m => m.List("owner", "repo", It.IsAny<WorkflowRunsRequest>(), It.IsAny<ApiOptions>()))
            .ReturnsAsync(response);

        var workflowsClient = new Mock<IActionsWorkflowsClient>();
        workflowsClient.SetupGet(w => w.Runs).Returns(runsClient.Object);

        var actionsClient = new Mock<IActionsClient>();
        actionsClient.SetupGet(a => a.Workflows).Returns(workflowsClient.Object);

        var ghClient = new Mock<IGitHubClient>();
        ghClient.SetupGet(c => c.Actions).Returns(actionsClient.Object);

        var factory = new Mock<IGitHubClientFactory>();
        factory.Setup(f => f.Create(It.IsAny<string>())).Returns(ghClient.Object);

        var svc = new GhaService(factory.Object);
        var result = await svc.GetLatestRunsAsync("token", new[] { "owner/repo" });

        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Repo.Should().Be("owner/repo");
        dto.Status.Should().Be("completed");
        dto.Conclusion.Should().Be("success");
        dto.Url.Should().Contain("/actions");
    }

    [Fact]
    public async Task GetLatestRunsAsync_InvalidRepo_IsSkipped()
    {
        var factory = new Mock<IGitHubClientFactory>();
        var svc = new GhaService(factory.Object);

        var result = await svc.GetLatestRunsAsync("token", new[] { "invalid-name" });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestRunsAsync_NotFound_ReturnsErrorDto()
    {
        var runsClient = new Mock<IActionsWorkflowRunsClient>();
        runsClient.Setup(m => m.List("no", "repo", It.IsAny<WorkflowRunsRequest>(), It.IsAny<ApiOptions>()))
            .ThrowsAsync(new NotFoundException("not found", System.Net.HttpStatusCode.NotFound));

        var workflowsClient = new Mock<IActionsWorkflowsClient>();
        workflowsClient.SetupGet(w => w.Runs).Returns(runsClient.Object);

        var actionsClient = new Mock<IActionsClient>();
        actionsClient.SetupGet(a => a.Workflows).Returns(workflowsClient.Object);

        var ghClient = new Mock<IGitHubClient>();
        ghClient.SetupGet(c => c.Actions).Returns(actionsClient.Object);

        var factory = new Mock<IGitHubClientFactory>();
        factory.Setup(f => f.Create(It.IsAny<string>())).Returns(ghClient.Object);

        var svc = new GhaService(factory.Object);
        var result = await svc.GetLatestRunsAsync("token", new[] { "no/repo" });

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("error");
        result[0].Conclusion.Should().Be("not_found");
    }
}

