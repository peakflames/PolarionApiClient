using Xunit;
using Xunit.Abstractions;
using Polarion;
using Polarion.Tests.Helpers;
using FluentAssertions;
using FluentResults;

namespace Polarion.Tests.Integration;

public class PolarionClientTests : IAsyncLifetime
{
    private readonly TestConfiguration _config;
    private PolarionClient _client = null!;
    private readonly ITestOutputHelper _output;

    public PolarionClientTests(ITestOutputHelper output)
    {
        _output = output;
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
        var result = await _client.GetWorkItemByIdAsync("MD-149588");

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
    public async Task GetModulesInSpaceThin_ShouldReturnExpectedResults()
    {
        // Arrange
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;

        // Act
        var result = await _client.GetModulesInSpaceThinAsync(spaceName);

        // Assert
        result.IsSuccess.Should().BeTrue("Document search should succeed");
        var documents = result.Value;
        documents.Should().NotBeNull();
        documents.Should().NotBeEmpty("Search should return at least one work item");

        // Verify first item has expected fields
        var firstItem = documents.First();
        firstItem.Id.Should().NotBeNullOrEmpty();
        firstItem.Title.Should().NotBeNullOrEmpty();
        firstItem.Type.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetDocumentsSpaces_ShouldReturnExpectedResults()
    {
        // Arrange
        // (none)

        // Act
        var result = await _client.GetSpacesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue("Document search should succeed");
        var documentSpaces = result.Value;
        documentSpaces.Should().NotBeNull();
        documentSpaces.Should().NotBeEmpty("Search should return at least one work item");

        // Verify first item has expected fields
        var firstItem = documentSpaces.First();
        firstItem.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetModulesThin_ShouldReturnExpectedResults()
    {
        // Arrange
        // (none)

        // Act
        var result = await _client.GetModulesThinAsync("Archive", "Deck");
        result.Should().NotBeNull();
        var modules = result.Value;
        modules.Should().NotBeNull();
        modules.Should().NotBeEmpty();

        // Print module IDs to output
        _output.WriteLine("Module IDs:");
        foreach (var module in modules)
        {
            _output.WriteLine($"- {module.Id} ({module.Title})");
        }
    }

    [Fact]
    public async Task GetWorkItemsByModuleAsync_ShouldReturnExpectedResults()
    {
        // Arrange
        var workItemFilter = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemFilter;
        var customFieldList = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemCustomFieldList.ToList();
        var filter = PolarionFilter.Create(null, true, true, customFieldList, true);
        var moduleTitle = _config.TestScenarioData.GetWorktemsByModuleModuleTitle;

        // Act
        var result = await _client.GetWorkItemsByModuleAsync(moduleTitle, filter);
        result.Should().NotBeNull();
        var workItems = result.Value;
        workItems.Should().NotBeNull();
        workItems.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHierarchicalWorkItemsByModuleAsync_ShouldReturnExpectedResults()
    {
        // Arrange
        var workItemFilter = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemFilter;
        var customFieldList = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemCustomFieldList.ToList();
        var filter = PolarionFilter.Create(workItemFilter, true, true, customFieldList, true);
        var moduleTitle = _config.TestScenarioData.GetWorktemsByModuleModuleTitle;
        var workItemPrefix = _config.TestScenarioData.GetHierarchicalWorkItemsByModuleItemPrefix;
        // Act
        var result = await _client.GetHierarchicalWorkItemsByModuleAsync(workItemPrefix, moduleTitle, filter);
        result.Should().NotBeNull();

        var workItemHierarchy = result.Value;
        workItemHierarchy.Should().NotBeNull();
        workItemHierarchy.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExportModuleToMarkdownAsync_ShouldReturnExpectedResults()
    {
        // Arrange
        var workItemFilter = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemFilter;
        var customFieldList = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemCustomFieldList.ToList();
        var filter = PolarionFilter.Create(workItemFilter, true, true, customFieldList, true);
        var moduleTitle = _config.TestScenarioData.GetWorktemsByModuleModuleTitle;
        var workItemPrefix = _config.TestScenarioData.GetHierarchicalWorkItemsByModuleItemPrefix;
        var workItemTypeToShortNameMap = _config.TestScenarioData.ExportModuleToMarkdownWorkItemTypeToShortNameMap;

        // Act: Include WorkItem Identifiers
        var result = await _client.ExportModuleToMarkdownAsync(workItemPrefix, moduleTitle, filter, workItemTypeToShortNameMap);
        result.Should().NotBeNull();
        var stringBuilder = result.Value;
        stringBuilder.Should().NotBeNull();
        var markdownDocumentContent = stringBuilder.ToString();
        markdownDocumentContent.Should().NotBeNullOrEmpty();
        markdownDocumentContent.Should().Contain(", type='paragraph'):__");
        markdownDocumentContent.Should().Contain("WorkItem(id='");

        // Act: Don't Include WorkItem Identifiers
        result = await _client.ExportModuleToMarkdownAsync(workItemPrefix, moduleTitle, filter, workItemTypeToShortNameMap, false);
        result.Should().NotBeNull();
        stringBuilder = result.Value;
        stringBuilder.Should().NotBeNull();
        markdownDocumentContent = stringBuilder.ToString();
        markdownDocumentContent.Should().NotBeNullOrEmpty();
        markdownDocumentContent.Should().NotContain(", type='paragraph'):__");
        markdownDocumentContent.Should().NotContain("WorkItem(id='");
    }

    [Fact]
    public async Task ExportModuleToMarkdownGroupedByHeadingAsync_ShouldReturnExpectedResults()
    {
        // Arrange
        var workItemFilter = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemFilter;
        var customFieldList = _config.TestScenarioData.GetWorktemsByModuleModuleWorkItemCustomFieldList.ToList();
        var filter = PolarionFilter.Create(workItemFilter, true, true, customFieldList, true);
        var moduleTitle = _config.TestScenarioData.GetWorktemsByModuleModuleTitle;
        var workItemPrefix = _config.TestScenarioData.GetHierarchicalWorkItemsByModuleItemPrefix;
        var workItemTypeToShortNameMap = _config.TestScenarioData.ExportModuleToMarkdownWorkItemTypeToShortNameMap;

        // Act: Include WorkItem Identifiers
        var result = await _client.ExportModuleToMarkdownGroupedByHeadingAsync(4, workItemPrefix, moduleTitle, filter, workItemTypeToShortNameMap);
        result.Should().NotBeNull();

        var headingGroupedMarkdown = result.Value;
        headingGroupedMarkdown.Should().NotBeNull();
        headingGroupedMarkdown.Should().NotBeEmpty();

        var countOfWorkItemWithExpectedWorkItemIdentifiers = 0;
        foreach (var markdownBuilder in headingGroupedMarkdown.Values)
        {
            markdownBuilder.Should().NotBeNull();
            var markdownDocumentContent = markdownBuilder.ToString();
            markdownDocumentContent.Should().NotBeNullOrEmpty();
            if (markdownDocumentContent.Contains("__WorkItem(id='"))
            {
                countOfWorkItemWithExpectedWorkItemIdentifiers++;
            }
        }

        // Assert
        countOfWorkItemWithExpectedWorkItemIdentifiers.Should().BeGreaterThan(0);

        // Act: Don't Include WorkItem Identifiers
        result = await _client.ExportModuleToMarkdownGroupedByHeadingAsync(4, workItemPrefix, moduleTitle, filter, workItemTypeToShortNameMap, false);
        result.Should().NotBeNull();

        headingGroupedMarkdown = result.Value;
        headingGroupedMarkdown.Should().NotBeNull();
        headingGroupedMarkdown.Should().NotBeEmpty();

        foreach (var markdownBuilder in headingGroupedMarkdown.Values)
        {
            markdownBuilder.Should().NotBeNull();
            var markdownDocumentContent = markdownBuilder.ToString();
            markdownDocumentContent.Should().NotBeNullOrEmpty();
            markdownDocumentContent.Should().NotContain("__WorkItem(id='");
        }
    }


    [Fact]
    public async Task ConvertWorkItemToMarkdown_ShouldReturnExpectedResults()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;


        var result = await _client.GetWorkItemByIdAsync(workItemId);
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var workItem = result.Value;
        workItem.Should().NotBeNull();

        // Act: Include WorkItem Identifiers
        var markdownContent = _client.ConvertWorkItemToMarkdown(workItemId, workItem, null, true);

        // Assert
        markdownContent.Should().NotBeNullOrEmpty();
        markdownContent.Should().Contain("__WorkItem(id='");
        

        // Act: Don't Include WorkItem Identifiers
        markdownContent = _client.ConvertWorkItemToMarkdown(workItemId, workItem, null, false);
        
        // Assert
        markdownContent.Should().NotBeNullOrEmpty();
        markdownContent.Should().NotContain("__WorkItem(id='");
        
    }

}
