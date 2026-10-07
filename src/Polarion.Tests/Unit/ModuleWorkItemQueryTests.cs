using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;
using static Polarion.Tests.Unit.GetModuleWorkItemsTests;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for QueryWorkItemsInModuleAsync and GetWorkItemsByModuleRevisionAsync,
/// which are built on getModuleWorkItems - no server connection required.
/// </summary>
public class ModuleWorkItemQueryTests
{
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#Doc";

    private static (PolarionClient Client, FakeService Tracker) CreateClient(params WorkItem[] rows)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module { uri = ModuleUri }));
        tracker.On<getModuleWorkItemsRequest, getModuleWorkItemsResponse>(
            "getModuleWorkItemsAsync", _ => new getModuleWorkItemsResponse(rows));
        return (client, tracker);
    }

    private static WorkItem Typed(WorkItem row, string type)
    {
        row.type = new EnumOptionId { id = type };
        return row;
    }

    #region QueryWorkItemsInModuleAsync

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ReturnsDocumentOrder_AndIgnoresSort()
    {
        var (client, tracker) = CreateClient(Row("WI-3", "2"), Row("OTHER-1", "9-4"), Row("WI-1", "1"));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc", sort: "id");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(w => w.id).Should().Equal("WI-3", "OTHER-1", "WI-1");
        tracker.RequestsFor<getModuleByLocationRequest>("getModuleByLocationAsync").Single().location.Should().Be("Space/Doc");
        tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single().moduleURI.Should().Be(ModuleUri);
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_DropsUnresolvableRows()
    {
        var (client, _) = CreateClient(Row("WI-1", "1"), UnresolvableRow("WI-404"), Row("WI-2", "2"));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.Value.Select(w => w.id).Should().Equal("WI-1", "WI-2");
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ReturnsPinnedValues()
    {
        var (client, _) = CreateClient(Row("WI-1", "1", revision: "777", status: "inReview"));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.Value.Single().status.id.Should().Be("inReview");
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ItemTypes_FiltersClientSideAndRequestsType()
    {
        var (client, tracker) = CreateClient(
            Typed(Row("WI-1", "1"), "heading"),
            Typed(Row("WI-2", "1-1"), "requirement"),
            Typed(Row("WI-3", "1-2"), "testCase"));
        var fields = new List<string> { "id", "title" };

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc", ["requirement", "testCase"], fields: fields);

        result.Value.Select(w => w.id).Should().Equal("WI-2", "WI-3");
        tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single().fields
            .Should().Equal("id", "title", "type");
        fields.Should().Equal(["id", "title"], "the caller's list must not be mutated");
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_EmptyDocument_ReturnsEmptySuccess()
    {
        var (client, _) = CreateClient();

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ModuleLookupFails_Fails()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => throw new InvalidOperationException("no such document"));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Space/Doc");
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ModuleWithoutUri_Fails()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module()));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("has no URI");
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_UnresolvableModule_FailsWithoutReadingRows()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module { uri = ModuleUri, unresolvable = true }));

        var result = await client.QueryWorkItemsInModuleAsync("Space", "Doc");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Space/Doc").And.Contain("not found");
        tracker.Calls.Select(c => c.Method).Should().NotContain("getModuleWorkItemsAsync");
    }

    #endregion

    #region GetWorkItemsByModuleRevisionAsync

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_QueriesModuleAtRevision_InDocumentOrder()
    {
        var (client, tracker) = CreateClient(Row("WI-9", "1"), Row("WI-2", "2"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(w => w.WorkItem.id).Should().Equal("WI-9", "WI-2");
        tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single().moduleURI
            .Should().Be($"{ModuleUri}%5000");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_PinnedItem_ReportsPinnedRevision()
    {
        var (client, _) = CreateClient(Row("WI-1", "1"), Row("WI-2", "2", revision: "4100", status: "approved"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        var unpinned = result.Value[0];
        unpinned.Revision.Should().Be("5000");
        unpinned.IsHistorical.Should().BeTrue();
        unpinned.SourceUri.Should().Be(ItemUri("WI-1"));

        var pinned = result.Value[1];
        pinned.Revision.Should().Be("4100");
        pinned.SourceUri.Should().Be(ItemUri("WI-2", "4100"));
        pinned.WorkItem.status.id.Should().Be("approved");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_AllRowsSuffixed_RevisionIsRequestedUnlessPinned()
    {
        // Polarion's revision-read shape: every row suffixed, unpinned ones with the requested revision.
        var (client, _) = CreateClient(
            Row("WI-1", "1", revision: "5000"),
            Row("WI-2", "2", revision: "4100", status: "approved"),
            Row("WI-3", "3", revision: "5000"),
            Row("WI-4", "4"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(w => w.Revision).Should().Equal("5000", "4100", "5000", "5000");
        result.Value.Select(w => w.SourceUri).Should().Equal(
            ItemUri("WI-1", "5000"), ItemUri("WI-2", "4100"), ItemUri("WI-3", "5000"), ItemUri("WI-4"));
        result.Value[1].WorkItem.status.id.Should().Be("approved");
        result.Value.Should().OnlyContain(w => w.IsHistorical);
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_DropsUnresolvableRows()
    {
        var (client, _) = CreateClient(UnresolvableRow("WI-404"), Row("WI-1", "1"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.Value.Select(w => w.WorkItem.id).Should().Equal("WI-1");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_UnresolvableModuleAtHead_StillReadsRevision()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module { uri = ModuleUri, unresolvable = true }));
        tracker.On<getModuleWorkItemsRequest, getModuleWorkItemsResponse>(
            "getModuleWorkItemsAsync", _ => new getModuleWorkItemsResponse([Row("WI-1", "1")]));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(w => w.WorkItem.id).Should().Equal("WI-1");
        tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single().moduleURI
            .Should().Be($"{ModuleUri}%5000");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_DoesNotMutateCallerFields()
    {
        var (client, tracker) = CreateClient(Row("WI-1", "1"));
        var fields = new List<string> { "id", "uri", "title" };

        await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000", fields);

        fields.Should().Equal("id", "uri", "title");
        tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single().fields
            .Should().Equal("id", "uri", "title");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_ServiceFault_FailsWithMessage()
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleByLocationRequest, getModuleByLocationResponse>(
            "getModuleByLocationAsync", _ => new getModuleByLocationResponse(new Module { uri = ModuleUri }));
        tracker.On<getModuleWorkItemsRequest, getModuleWorkItemsResponse>(
            "getModuleWorkItemsAsync", _ => throw new InvalidOperationException("revision does not exist"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "999999999");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("999999999").And.Contain("revision does not exist");
    }

    [Theory]
    [InlineData("", "Doc", "1")]
    [InlineData("Space", "", "1")]
    [InlineData("Space", "Doc", "")]
    public async Task GetWorkItemsByModuleRevisionAsync_MissingArguments_Fail(string folder, string doc, string revision)
    {
        var (client, tracker) = FakeClient.Create();

        var result = await client.GetWorkItemsByModuleRevisionAsync(folder, doc, revision);

        result.IsFailed.Should().BeTrue();
        tracker.Calls.Should().BeEmpty();
    }

    #endregion
}
