using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for QueryBaselinesAsync and QueryModuleUrisInBaselineAsync - no server connection required.
/// </summary>
public class BaselineQueryTests
{
    #region QueryBaselinesAsync

    [Fact]
    public async Task QueryBaselinesAsync_ForwardsQueryUnchanged_AndReturnsBaselines()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryBaselinesRequest, queryBaselinesResponse>("queryBaselinesAsync", _ => new queryBaselinesResponse([
            new Baseline { id = "b-1", name = "Release 1", baseRevision = "1000" },
            new Baseline { id = "b-2", name = "Release 2", baseRevision = "2000" },
        ]));

        var result = await client.QueryBaselinesAsync("project.id:TestProject AND name:Release*");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(b => b.baseRevision).Should().Equal("1000", "2000");
        var request = tracker.RequestsFor<queryBaselinesRequest>("queryBaselinesAsync").Single();
        request.query.Should().Be("project.id:TestProject AND name:Release*");
        request.sort.Should().Be("baseRevision");
    }

    [Fact]
    public async Task QueryBaselinesAsync_NoMatches_ReturnsEmptySuccess()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryBaselinesRequest, queryBaselinesResponse>("queryBaselinesAsync", _ => new queryBaselinesResponse(null!));

        var result = await client.QueryBaselinesAsync("name:none", "name");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        tracker.RequestsFor<queryBaselinesRequest>("queryBaselinesAsync").Single().sort.Should().Be("name");
    }

    [Fact]
    public async Task QueryBaselinesAsync_Fault_FailsWithMessage()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryBaselinesRequest, queryBaselinesResponse>("queryBaselinesAsync", _ => throw new InvalidOperationException("bad lucene"));

        var result = await client.QueryBaselinesAsync("name:(");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("bad lucene");
    }

    [Fact]
    public async Task QueryBaselinesAsync_EmptyQuery_Fails()
    {
        var (client, tracker) = FakeClient.Create();

        var result = await client.QueryBaselinesAsync(" ");

        result.IsFailed.Should().BeTrue();
        tracker.Calls.Should().BeEmpty();
    }

    #endregion

    #region QueryModuleUrisInBaselineAsync

    [Fact]
    public async Task QueryModuleUrisInBaselineAsync_ForwardsArguments_AndReturnsUris()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryModuleUrisInBaselineRequest, queryModuleUrisInBaselineResponse>(
            "queryModuleUrisInBaselineAsync", _ => new queryModuleUrisInBaselineResponse(["uri-a", "uri-b"]));

        var result = await client.QueryModuleUrisInBaselineAsync("3000", "project.id:TestProject", "id", 50);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal("uri-a", "uri-b");
        var request = tracker.RequestsFor<queryModuleUrisInBaselineRequest>("queryModuleUrisInBaselineAsync").Single();
        request.baselineRevision.Should().Be("3000");
        request.query.Should().Be("project.id:TestProject");
        request.sort.Should().Be("id");
        request.resultsLimit.Should().Be(50);
    }

    [Fact]
    public async Task QueryModuleUrisInBaselineAsync_Defaults_SortByUriAndNoLimit()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryModuleUrisInBaselineRequest, queryModuleUrisInBaselineResponse>(
            "queryModuleUrisInBaselineAsync", _ => new queryModuleUrisInBaselineResponse(null!));

        var result = await client.QueryModuleUrisInBaselineAsync("3000", "project.id:TestProject");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        var request = tracker.RequestsFor<queryModuleUrisInBaselineRequest>("queryModuleUrisInBaselineAsync").Single();
        request.sort.Should().Be("uri");
        request.resultsLimit.Should().Be(-1);
    }

    [Fact]
    public async Task QueryModuleUrisInBaselineAsync_Fault_FailsWithMessage()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryModuleUrisInBaselineRequest, queryModuleUrisInBaselineResponse>(
            "queryModuleUrisInBaselineAsync", _ => throw new InvalidOperationException("unknown revision"));

        var result = await client.QueryModuleUrisInBaselineAsync("3000", "project.id:TestProject");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("unknown revision");
    }

    [Theory]
    [InlineData("", "q")]
    [InlineData("3000", "")]
    public async Task QueryModuleUrisInBaselineAsync_MissingArguments_Fail(string revision, string query)
    {
        var (client, tracker) = FakeClient.Create();

        var result = await client.QueryModuleUrisInBaselineAsync(revision, query);

        result.IsFailed.Should().BeTrue();
        tracker.Calls.Should().BeEmpty();
    }

    #endregion
}
