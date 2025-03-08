using Xunit;
using Polarion;
using Polarion.Tests.Helpers;
using FluentAssertions;

namespace Polarion.Tests.Integration;

public class PolarionClientTests : IAsyncLifetime
{
    private readonly TestConfiguration _config;
    private PolarionClient _client = null!;

    public PolarionClientTests()
    {
        // Load configuration from the test settings file
        _config = TestConfigurationLoader.Load("../../../appsettings.test.json");
    }

    public async Task InitializeAsync()
    {
        // Create the client before each test
        _client = await PolarionClient.CreateAsync(_config.PolarionClient);
        _client.Should().NotBeNull("Polarion client should be created successfully");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetWorkItemById_ShouldReturnValidWorkItem()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        // Act
        var result = await _client.GetWorkItemByIdAsync(workItemId);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var workItem = result.Value;
        workItem.Should().NotBeNull();
        workItem.id.Should().NotBeNullOrEmpty();
        workItem.title.Should().NotBeNullOrEmpty();
        workItem.type.id.Should().NotBeNullOrEmpty();
        workItem.status.id.Should().NotBeNullOrEmpty();
        workItem.author.name.Should().NotBeNullOrEmpty();
        workItem.description.content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchWorkitem_ShouldReturnExpectedResults()
    {
        // Arrange
        var query = _config.TestScenarioData.SearchWorkitemQuery;
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();

        // Act
        var result = await _client.SearchWorkitem(
            query: query,
            order: order,
            field_list: fieldList
        );

        // Assert
        result.IsSuccess.Should().BeTrue("Work item search should succeed");
        var workItems = result.Value;
        workItems.Should().NotBeNull();
        workItems.Should().NotBeEmpty("Search should return at least one work item");
        
        // Verify first item has expected fields
        var firstItem = workItems.First();
        firstItem.id.Should().NotBeNullOrEmpty();
        firstItem.title.Should().NotBeNullOrEmpty();
        firstItem.type.id.Should().NotBeNullOrEmpty();
    }
}
