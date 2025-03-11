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
        var result = await PolarionClient.CreateAsync(_config.PolarionClient);
        result.IsSuccess.Should().BeTrue("Polarion client should be created successfully");
        _client = result.Value;
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
        var result = await _client.SearchWorkitemAsync(
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

    [Fact]
    public async Task SearchWorkitem_ShouldGracfullyFail_Type1()
    {
        // Arrange
        var query = "";
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();
        var baselineRevision = _config.TestScenarioData.SearchWorkitemBaselineRevision;

        // Act
        var result = await _client.SearchWorkitemAsync(
            query: query,
            order: order,
            field_list: fieldList
        );

        // Assert
        result.IsSuccess.Should().BeFalse("Work item search is expected to failed");
    }

    [Fact]
    public async Task SearchWorkitem_ShouldGracfullyFail_Type2()
    {
        // Arrange
        var query = "(type:softwareHLR OR type:heading OR type:paragraph) AND document.title:\"Your Module Title\"";
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();
        var baselineRevision = _config.TestScenarioData.SearchWorkitemBaselineRevision;

        // Act
        var result = await _client.SearchWorkitemAsync(
            query: query,
            order: order,
            field_list: fieldList
        );

        // Assert
        result.IsSuccess.Should().BeFalse("Work item search is expected to failed");
    }

    [Fact]
    public async Task SearchWorkitemInBaseline_ShouldReturnExpectedResults()
    {
        // Arrange
        var query = _config.TestScenarioData.SearchWorkitemQuery;
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();
        var baselineRevision = _config.TestScenarioData.SearchWorkitemBaselineRevision;

        // Act
        var result = await _client.SearchWorkitemInBaselineAsync(
            baselineRevision: baselineRevision,
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

    [Fact]
    public async Task SearchWorkitemInBaseline_ShouldGracfullyFail_Type1()
    {
        // Arrange
        var query = "";
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();
        var baselineRevision = _config.TestScenarioData.SearchWorkitemBaselineRevision;

        // Act
        var result = await _client.SearchWorkitemInBaselineAsync(
            baselineRevision: baselineRevision,
            query: query,
            order: order,
            field_list: fieldList
        );

        // Assert
        result.IsSuccess.Should().BeFalse("Work item search is expected to failed");
    }

    [Fact]
    public async Task SearchWorkitemInBaseline_ShouldGracfullyFail_Type2()
    {
        // Arrange
        var query = "(type:softwareHLR OR type:heading OR type:paragraph) AND document.title:\"Your Module Title\"";
        var order = _config.TestScenarioData.SearchWorkitemOrder;
        var fieldList = _config.TestScenarioData.SearchWorkitemFieldList.ToList();
        var baselineRevision = _config.TestScenarioData.SearchWorkitemBaselineRevision;

        // Act
        var result = await _client.SearchWorkitemInBaselineAsync(
            baselineRevision: baselineRevision,
            query: query,
            order: order,
            field_list: fieldList
        );

        // Assert
        result.IsSuccess.Should().BeFalse("Work item search is expected to failed");
    }


    [Fact]
    public async Task GetDocumentsInSpace_ShouldReturnExpectedResults()
    {
        // Arrange
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;

        // Act
        var result = await _client.GetDocumentsInSpaceAsync(spaceName); 
        
        // Assert
        result.IsSuccess.Should().BeTrue("Document search should succeed");
        var documents = result.Value;
        documents.Should().NotBeNull();
        documents.Should().NotBeEmpty("Search should return at least one work item");
        
        // Verify first item has expected fields
        var firstItem = documents.First();
        firstItem.id.Should().NotBeNullOrEmpty();
        firstItem.title.Should().NotBeNullOrEmpty();
        firstItem.type.id.Should().NotBeNullOrEmpty();
    }
}
