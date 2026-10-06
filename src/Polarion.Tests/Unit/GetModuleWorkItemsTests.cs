using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for GetModuleWorkItemsAsync - no server connection required.
/// </summary>
public class GetModuleWorkItemsTests
{
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#Doc";

    internal static string ItemUri(string id, string? revision = null, string project = FakeClient.ProjectId) =>
        $"subterra:data-service:objects:/default/{project}${{WorkItem}}{id}" + (revision is null ? string.Empty : $"%{revision}");

    internal static WorkItem Row(string id, string outline, string? revision = null, string status = "draft") => new()
    {
        id = id,
        uri = ItemUri(id, revision),
        outlineNumber = outline,
        status = new EnumOptionId { id = status },
        type = new EnumOptionId { id = "requirement" },
    };

    internal static WorkItem UnresolvableRow(string id) => new()
    {
        uri = ItemUri(id),
        unresolvable = true,
        unresolvableSpecified = true,
    };

    private static (PolarionClient Client, FakeService Tracker) CreateClient(Func<getModuleWorkItemsRequest, getModuleWorkItemsResponse> handler)
    {
        var (client, tracker) = FakeClient.Create();
        tracker.On("getModuleWorkItemsAsync", handler);
        return (client, tracker);
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_PreservesServerOrder_EvenWhenOutlineNumbersDisagree()
    {
        var (client, _) = CreateClient(_ => new getModuleWorkItemsResponse([
            Row("WI-3", "1-3"),
            Row("OTHER-9", "7-1"),
            Row("WI-1", "1-1"),
        ]));

        var result = await client.GetModuleWorkItemsAsync(ModuleUri);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(r => r.Id).Should().Equal("WI-3", "OTHER-9", "WI-1");
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_PinnedRow_ExposesRevisionAndPinnedValues()
    {
        var (client, _) = CreateClient(_ => new getModuleWorkItemsResponse([
            Row("WI-1", "1-1"),
            Row("WI-2", "1-2", revision: "4242", status: "inReview"),
        ]));

        var result = await client.GetModuleWorkItemsAsync(ModuleUri);

        var rows = result.Value;
        rows[0].Revision.Should().BeEmpty();
        rows[0].IsPinned.Should().BeFalse();
        rows[1].Revision.Should().Be("4242");
        rows[1].IsPinned.Should().BeTrue();
        rows[1].Id.Should().Be("WI-2");
        rows[1].Uri.Should().Be(ItemUri("WI-2", "4242"));
        rows[1].WorkItem.status.id.Should().Be("inReview");
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_UnresolvableRow_IsReturnedAndFlagged()
    {
        var (client, _) = CreateClient(_ => new getModuleWorkItemsResponse([
            Row("WI-1", "1-1"),
            UnresolvableRow("WI-404"),
        ]));

        var result = await client.GetModuleWorkItemsAsync(ModuleUri);

        result.Value.Should().HaveCount(2);
        result.Value[0].IsUnresolvable.Should().BeFalse();
        result.Value[1].IsUnresolvable.Should().BeTrue();
        result.Value[1].Id.Should().Be("WI-404", "the ID is parsed from the URI when no fields are returned");
        result.Value[1].WorkItem.title.Should().BeNull();
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_ForwardsArguments()
    {
        var (client, tracker) = CreateClient(_ => new getModuleWorkItemsResponse([]));
        var fields = new List<string> { "id", "title" };

        await client.GetModuleWorkItemsAsync($"{ModuleUri}%1234", "parent-uri", deep: false, fields);

        var request = tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single();
        request.moduleURI.Should().Be($"{ModuleUri}%1234");
        request.parentWorkItemURI.Should().Be("parent-uri");
        request.deep.Should().BeFalse();
        request.fields.Should().Equal("id", "title");
        fields.Should().Equal(["id", "title"], "the caller's list must not be mutated");
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_NoFields_UsesDefaultsAndDeep()
    {
        var (client, tracker) = CreateClient(_ => new getModuleWorkItemsResponse([]));

        await client.GetModuleWorkItemsAsync(ModuleUri);

        var request = tracker.RequestsFor<getModuleWorkItemsRequest>("getModuleWorkItemsAsync").Single();
        request.deep.Should().BeTrue();
        request.parentWorkItemURI.Should().BeNull();
        request.fields.Should().Contain(["id", "type", "title", "status", "outlineNumber"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetModuleWorkItemsAsync_EmptyOrNullResponse_ReturnsEmptySuccess(bool nullArray)
    {
        var (client, _) = CreateClient(_ => new getModuleWorkItemsResponse(nullArray ? null! : []));

        var result = await client.GetModuleWorkItemsAsync(ModuleUri);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModuleWorkItemsAsync_Fault_FailsWithMessage()
    {
        var (client, _) = CreateClient(_ => throw new InvalidOperationException("module not found"));

        var result = await client.GetModuleWorkItemsAsync(ModuleUri);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("module not found");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetModuleWorkItemsAsync_EmptyModuleUri_Fails(string moduleUri)
    {
        var (client, tracker) = FakeClient.Create();

        var result = await client.GetModuleWorkItemsAsync(moduleUri);

        result.IsFailed.Should().BeTrue();
        tracker.Calls.Should().BeEmpty();
    }
}
