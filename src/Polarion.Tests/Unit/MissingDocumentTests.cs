using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;
using static Polarion.Tests.Unit.GetModuleWorkItemsTests;

namespace Polarion.Tests.Unit;

/// <summary>
/// A module read that returns no rows is an empty document only when the module resolves. When
/// Polarion reports the module unresolvable (no document at that URI or revision), the read fails -
/// no server connection required.
/// </summary>
public class MissingDocumentTests
{
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#Doc";

    private static FakeService WithModuleByUri(FakeService tracker, bool unresolvable) =>
        tracker.On<getModuleByUriRequest, getModuleByUriResponse>(
            "getModuleByUriAsync", req => new getModuleByUriResponse(new Module { uri = req.uri, unresolvable = unresolvable }));

    private static (PolarionClient Client, FakeService Tracker) RevisionClient(
        WorkItem[] rows, bool? unresolvableAtRevision)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleWorkItemsRequest, getModuleWorkItemsResponse>(
            "getModuleWorkItemsAsync", _ => new getModuleWorkItemsResponse(rows));
        if (unresolvableAtRevision is { } flag)
        {
            WithModuleByUri(tracker, flag);
        }

        return (client, tracker);
    }

    private static (PolarionClient Client, FakeService Tracker) UrisClient(string[]? uris, bool? unresolvable)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On<getModuleWorkItemUrisRequest, getModuleWorkItemUrisResponse>(
            "getModuleWorkItemUrisAsync", _ => new getModuleWorkItemUrisResponse(uris!));
        if (unresolvable is { } flag)
        {
            WithModuleByUri(tracker, flag);
        }

        return (client, tracker);
    }

    #region GetWorkItemsByModuleRevisionAsync

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_NoRows_UnresolvableModule_Fails()
    {
        // A mistyped document ID: unresolvable at HEAD and at the revision, and no rows.
        var (client, tracker) = RevisionClient([], unresolvableAtRevision: true);

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 5000").And.Contain("Space/Doc");
        tracker.RequestsFor<getModuleByUriRequest>("getModuleByUriAsync").Single().uri.Should().Be($"{ModuleUri}%5000");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_NoRows_RevisionBeforeDocumentExisted_Fails()
    {
        // The document exists at HEAD but not at the requested revision.
        var (client, _) = RevisionClient([], unresolvableAtRevision: true);

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 5000");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_NoRows_ResolvableModule_ReturnsEmptySuccess()
    {
        var (client, _) = RevisionClient([], unresolvableAtRevision: false);

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_NoRows_LookupFails_Fails()
    {
        var (client, tracker) = RevisionClient([], unresolvableAtRevision: null);
        tracker.On<getModuleByUriRequest, getModuleByUriResponse>(
            "getModuleByUriAsync", _ => throw new InvalidOperationException("lookup rejected"));

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found at revision 5000").And.Contain("lookup rejected");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_WithRows_DoesNotLookUpModule()
    {
        var (client, tracker) = RevisionClient([Row("WI-1", "1")], unresolvableAtRevision: null);

        var result = await client.GetWorkItemsByModuleRevisionAsync("Space", "Doc", "5000");

        result.IsSuccess.Should().BeTrue();
        tracker.Calls.Select(c => c.Method).Should().NotContain("getModuleByUriAsync");
    }

    #endregion

    #region GetModuleWorkItemUrisAsync

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetModuleWorkItemUrisAsync_NoUris_UnresolvableModule_Fails(bool nullArray)
    {
        var uri = $"{ModuleUri}%5000";
        var (client, tracker) = UrisClient(nullArray ? null : [], unresolvable: true);

        var result = await client.GetModuleWorkItemUrisAsync(uri);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("Document not found").And.Contain(uri);
        tracker.RequestsFor<getModuleByUriRequest>("getModuleByUriAsync").Single().uri.Should().Be(uri);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetModuleWorkItemUrisAsync_NoUris_ResolvableModule_ReturnsEmptySuccess(bool nullArray)
    {
        var (client, _) = UrisClient(nullArray ? null : [], unresolvable: false);

        var result = await client.GetModuleWorkItemUrisAsync(ModuleUri);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetModuleWorkItemUrisAsync_WithUris_DoesNotLookUpModule()
    {
        var (client, tracker) = UrisClient([ItemUri("WI-1")], unresolvable: null);

        var result = await client.GetModuleWorkItemUrisAsync(ModuleUri);

        result.Value.Should().Equal(ItemUri("WI-1"));
        tracker.Calls.Select(c => c.Method).Should().NotContain("getModuleByUriAsync");
    }

    #endregion
}
