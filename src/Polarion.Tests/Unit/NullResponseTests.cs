using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Polarion answers a query or lookup that matches nothing with an empty body, which WCF hands back
/// as a null array. These tests check that every array-returning call maps that to an empty
/// success instead of dereferencing null - no server connection required.
/// </summary>
public class NullResponseTests
{
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#Doc";
    private const string ItemUri = "subterra:data-service:objects:/default/TestProject${WorkItem}WI-1";

    private static (PolarionClient Client, FakeService Tracker) ClientWithModules(Module[]? rows)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryModulesBySQLRequest, queryModulesBySQLResponse>(
            "queryModulesBySQLAsync", _ => new queryModulesBySQLResponse(rows!));
        return (client, tracker);
    }

    private static (PolarionClient Client, FakeService Tracker) ClientWithNullRevisions()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getRevisionsRequest, getRevisionsResponse>("getRevisionsAsync", _ => new getRevisionsResponse(null!));
        tracker.On<getWorkItemByIdRequest, getWorkItemByIdResponse>(
            "getWorkItemByIdAsync", _ => new getWorkItemByIdResponse(new WorkItem { id = "WI-1", uri = ItemUri }));
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module { uri = ModuleUri }));
        return (client, tracker);
    }

    #region queryModulesBySQL

    [Fact]
    public async Task GetModulesInSpaceThinAsync_NullArray_ReturnsEmptySuccess()
    {
        var (client, _) = ClientWithModules(null);

        var result = await client.GetModulesInSpaceThinAsync("O'Brien");

        result.IsSuccess.Should().BeTrue(result.IsFailed ? result.Errors[0].Message : "");
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModulesThinAsync_NullArray_ReturnsEmptySuccess()
    {
        var (client, _) = ClientWithModules(null);

        var result = await client.GetModulesThinAsync(titleContains: "O'Brien");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModulesInSpaceThinAsync_RowsWithNullsInside_AreMappedOrSkipped()
    {
        var (client, _) = ClientWithModules(
        [
            new Module { id = "B", title = "Beta", uri = ModuleUri, moduleFolder = "Space", moduleLocation = "Space/B" },
            null!,
            new Module { id = null, title = "No id" },
            new Module { id = "A", title = "Alpha", type = new EnumOptionId { id = "req" }, status = new EnumOptionId { id = "draft" } },
        ]);

        var result = await client.GetModulesInSpaceThinAsync("Space");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(m => m.Id).Should().Equal("A", "B");
        result.Value[0].Type.Should().Be("req");
        result.Value[0].Status.Should().Be("draft");
        result.Value[1].Type.Should().BeEmpty();
        result.Value[1].Status.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModulesThinAsync_RowsWithNullTypeAndStatus_AreMapped()
    {
        var (client, _) = ClientWithModules([new Module { id = "A", title = "Alpha" }]);

        var result = await client.GetModulesThinAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Single().Type.Should().BeEmpty();
    }

    #endregion

    #region Other array-returning calls

    [Theory]
    [InlineData(null)]
    [InlineData("x")]
    public async Task GetSpacesAsync_NullArray_ReturnsEmptySuccess(string? exclude)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getDocumentSpacesRequest, getDocumentSpacesResponse>(
            "getDocumentSpacesAsync", _ => new getDocumentSpacesResponse(null!));

        var result = await client.GetSpacesAsync(exclude);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModuleWorkItemUrisAsync_NullArray_ReturnsEmptySuccess()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleWorkItemUrisRequest, getModuleWorkItemUrisResponse>(
            "getModuleWorkItemUrisAsync", _ => new getModuleWorkItemUrisResponse(null!));
        tracker.On<getModuleByUriRequest, getModuleByUriResponse>(
            "getModuleByUriAsync", _ => new getModuleByUriResponse(new Module { uri = ModuleUri }));

        var result = await client.GetModuleWorkItemUrisAsync(ModuleUri);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRevisionIdsAsync_NullArray_ReturnsEmptyArray()
    {
        var (client, _) = ClientWithNullRevisions();

        var result = await client.GetRevisionIdsAsync(ItemUri);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetRevisionsIdsByWorkItemIdAsync_NullArray_ReturnsEmptyArray()
    {
        var (client, _) = ClientWithNullRevisions();

        var result = await client.GetRevisionsIdsByWorkItemIdAsync("WI-1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetWorkItemRevisionsByIdAsync_NullArray_ReturnsEmptySuccess()
    {
        var (client, _) = ClientWithNullRevisions();

        var result = await client.GetWorkItemRevisionsByIdAsync("WI-1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModuleRevisionsByLocationAsync_NullArray_ReturnsEmptySuccess()
    {
        var (client, _) = ClientWithNullRevisions();

        var result = await client.GetModuleRevisionsByLocationAsync("Space/Doc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion
}
