using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for work item query result handling - no server connection required.
/// </summary>
public class SearchWorkitemTests
{
    private static (PolarionClient Client, FakeService Tracker) CreateClient(Func<queryWorkItemsRequest, queryWorkItemsResponse> handler)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On("queryWorkItemsAsync", handler);
        return (client, tracker);
    }

    public static TheoryData<string> Variants => new() { "default", "simple", "complex" };

    private static Task<FluentResults.Result<WorkItem[]>> Search(PolarionClient client, string variant) => variant switch
    {
        "simple" => client.SearchWorkitemAsyncSimple("type:requirement"),
        "complex" => client.SearchWorkitemAsyncComplex("type:requirement"),
        _ => client.SearchWorkitemAsync("type:requirement"),
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task SearchWorkitemAsync_ZeroRows_ReturnsEmptySuccess(string variant)
    {
        var (client, _) = CreateClient(_ => new queryWorkItemsResponse(null!));

        var result = await Search(client, variant);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task SearchWorkitemAsync_SoapFault_FailsWithServerMessage(string variant)
    {
        var (client, _) = CreateClient(_ => throw new InvalidOperationException("syntax error at or near \"FROM\""));

        var result = await Search(client, variant);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("syntax error at or near \"FROM\"");
    }

    [Fact]
    public async Task SearchWorkitemAsync_Rows_ReturnsRowsAndScopesToProject()
    {
        var (client, tracker) = CreateClient(_ => new queryWorkItemsResponse([new WorkItem { id = "WI-1" }, new WorkItem { id = "WI-2" }]));

        var result = await client.SearchWorkitemAsync("type:requirement");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(w => w.id).Should().Equal("WI-1", "WI-2");
        tracker.RequestsFor<queryWorkItemsRequest>("queryWorkItemsAsync").Single().query
            .Should().Be($"type:requirement AND project.id:{FakeClient.ProjectId}");
    }

    [Fact]
    public async Task SearchWorkitemInBaselineAsync_ZeroRows_ReturnsEmptySuccess()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryWorkItemsInBaselineRequest, queryWorkItemsInBaselineResponse>(
            "queryWorkItemsInBaselineAsync", _ => new queryWorkItemsInBaselineResponse(null!));

        var result = await client.SearchWorkitemInBaselineAsync("1234", "type:requirement");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchWorkitemInBaselineAsync_SoapFault_FailsWithServerMessage()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryWorkItemsInBaselineRequest, queryWorkItemsInBaselineResponse>(
            "queryWorkItemsInBaselineAsync", _ => throw new InvalidOperationException("bad query"));

        var result = await client.SearchWorkitemInBaselineAsync("1234", "type:requirement");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("bad query");
    }
}
