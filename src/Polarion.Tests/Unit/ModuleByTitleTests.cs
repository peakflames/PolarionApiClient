using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

#pragma warning disable CS0618 // Exercises the deprecated GetWorkItemsByModuleAsync path on purpose

namespace Polarion.Tests.Unit;

/// <summary>
/// The title-based module reads (<see cref="PolarionClient.GetWorkItemsByModuleAsync"/> and the
/// hierarchical and Markdown exports built on it) return an empty success only when the module exists.
/// A zero-row query for a title that matches no module, or a module that does not exist at the requested
/// revision, fails - no server connection required.
/// </summary>
public class ModuleByTitleTests
{
    private const string Title = "My Module";
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#My Module";
    private static readonly PolarionFilter Filter = new("type:requirement", "id", ["id"]);

    private static Module ModuleRow(string title, string uri) => new() { id = title, title = title, uri = uri };

    private static (PolarionClient Client, FakeService Tracker) CreateClient(
        WorkItem[]? rows, Module[]? modules, bool? unresolvableAtRevision = null)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<queryWorkItemsRequest, queryWorkItemsResponse>("queryWorkItemsAsync", _ => new queryWorkItemsResponse(rows!));
        tracker.On<queryWorkItemsInBaselineRequest, queryWorkItemsInBaselineResponse>(
            "queryWorkItemsInBaselineAsync", _ => new queryWorkItemsInBaselineResponse(rows!));
        tracker.On<queryModulesBySQLRequest, queryModulesBySQLResponse>(
            "queryModulesBySQLAsync", _ => new queryModulesBySQLResponse(modules!));
        if (unresolvableAtRevision is { } flag)
        {
            tracker.On<getModuleByUriRequest, getModuleByUriResponse>(
                "getModuleByUriAsync", req => new getModuleByUriResponse(new Module { uri = req.uri, unresolvable = flag }));
        }

        return (client, tracker);
    }

    private static WorkItem Row(string id, string outline) => new() { id = id, outlineNumber = outline };

    #region GetWorkItemsByModuleAsync

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_NoModuleWithThatTitle_Fails()
    {
        var (client, _) = CreateClient(rows: null, modules: null);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found").And.Contain(Title);
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_LooksUpModulesByTitleAndEscapesIt()
    {
        var (client, tracker) = CreateClient(rows: null, modules: null);

        var result = await client.GetWorkItemsByModuleAsync("It's 100%", Filter);

        result.IsFailed.Should().BeTrue();
        var sql = tracker.RequestsFor<queryModulesBySQLRequest>("queryModulesBySQLAsync").Single().sqlQuery;
        sql.Should().Contain(@"IT''S 100\%").And.Contain("TestProject");
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_ModuleTitleHasSuffix_ReturnsEmptySuccess()
    {
        // The work item query matches document.title as a phrase, so "My Module" finds "My Module - Variant".
        var (client, _) = CreateClient(rows: null, modules: [ModuleRow("My Module - Variant", ModuleUri)]);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_ModuleExists_ReturnsEmptySuccess()
    {
        // A valid module whose filter matches nothing is an empty result, not a failure.
        var (client, tracker) = CreateClient(rows: null, modules: [ModuleRow("my module", ModuleUri)]);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        tracker.Calls.Select(c => c.Method).Should().NotContain("getModuleByUriAsync");
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_RowsWithoutOutlineNumber_ModuleExists_ReturnsEmptySuccess()
    {
        var (client, _) = CreateClient(rows: [new WorkItem { id = "WI-1" }], modules: [ModuleRow(Title, ModuleUri)]);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_WithRows_DoesNotLookUpTheModule()
    {
        var (client, tracker) = CreateClient(rows: [Row("WI-1", "1")], modules: null);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        tracker.Calls.Select(c => c.Method).Should().NotContain("queryModulesBySQLAsync");
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_ModuleLookupFails_Fails()
    {
        var (client, tracker) = CreateClient(rows: null, modules: null);
        tracker.On<queryModulesBySQLRequest, queryModulesBySQLResponse>(
            "queryModulesBySQLAsync", _ => throw new InvalidOperationException("sql rejected"));

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Failed to look up document").And.Contain("sql rejected");
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_Revision_ModuleUnresolvableAtRevision_Fails()
    {
        // The module exists at HEAD but did not exist at the requested revision.
        var (client, tracker) = CreateClient(rows: null, modules: [ModuleRow(Title, ModuleUri)], unresolvableAtRevision: true);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter, "1234");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 1234").And.Contain(Title);
        tracker.RequestsFor<getModuleByUriRequest>("getModuleByUriAsync").Single().uri.Should().Be($"{ModuleUri}%1234");
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_Revision_ModuleResolvableAtRevision_ReturnsEmptySuccess()
    {
        var (client, _) = CreateClient(rows: null, modules: [ModuleRow(Title, ModuleUri)], unresolvableAtRevision: false);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter, "1234");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_Revision_AnyMatchingModuleResolves_ReturnsEmptySuccess()
    {
        const string otherUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#My Module Two";
        var (client, tracker) = CreateClient(
            rows: null,
            modules: [ModuleRow("My Module Two", otherUri), ModuleRow(Title, ModuleUri)],
            unresolvableAtRevision: true);
        // Only the exact-title module exists at the revision; it is tried first.
        tracker.On<getModuleByUriRequest, getModuleByUriResponse>(
            "getModuleByUriAsync", req => new getModuleByUriResponse(new Module { uri = req.uri, unresolvable = req.uri != $"{ModuleUri}%1234" }));

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter, "1234");

        result.IsSuccess.Should().BeTrue();
        tracker.RequestsFor<getModuleByUriRequest>("getModuleByUriAsync").Should().ContainSingle();
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_Revision_NoMatchingModuleResolves_Fails()
    {
        const string otherUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#My Module Two";
        var (client, tracker) = CreateClient(
            rows: null,
            modules: [ModuleRow(Title, ModuleUri), ModuleRow("My Module Two", otherUri)],
            unresolvableAtRevision: true);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter, "1234");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 1234");
        tracker.RequestsFor<getModuleByUriRequest>("getModuleByUriAsync").Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_NoRows_Revision_NoModuleWithThatTitle_Fails()
    {
        var (client, tracker) = CreateClient(rows: null, modules: null, unresolvableAtRevision: false);

        var result = await client.GetWorkItemsByModuleAsync(Title, Filter, "1234");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found").And.Contain(Title);
        tracker.Calls.Select(c => c.Method).Should().NotContain("getModuleByUriAsync");
    }

    #endregion

    #region Hierarchical and Markdown exports

    [Fact]
    public async Task GetHierarchicalWorkItemsByModuleAsync_NoRows_NoModuleWithThatTitle_Fails()
    {
        var (client, _) = CreateClient(rows: null, modules: null);

        var result = await client.GetHierarchicalWorkItemsByModuleAsync("MD", Title, Filter);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found");
    }

    [Fact]
    public async Task ExportModuleToMarkdownAsync_NoRows_NoModuleWithThatTitle_Fails()
    {
        var (client, _) = CreateClient(rows: null, modules: null);

        var result = await client.ExportModuleToMarkdownAsync("MD", Title, Filter, []);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found");
    }

    [Fact]
    public async Task ExportModuleToMarkdownAsync_NoRows_Revision_ModuleUnresolvableAtRevision_Fails()
    {
        var (client, _) = CreateClient(rows: null, modules: [ModuleRow(Title, ModuleUri)], unresolvableAtRevision: true);

        var result = await client.ExportModuleToMarkdownAsync("MD", Title, Filter, [], revision: "1234");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 1234");
    }

    [Fact]
    public async Task ExportModuleToMarkdownAsync_NoRows_ModuleExists_ReturnsEmptySuccess()
    {
        var (client, _) = CreateClient(rows: null, modules: [ModuleRow(Title, ModuleUri)]);

        var result = await client.ExportModuleToMarkdownAsync("MD", Title, Filter, []);

        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(0);
    }

    #endregion
}
