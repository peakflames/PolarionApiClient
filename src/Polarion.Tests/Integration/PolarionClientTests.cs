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
    public async Task GetWorkItemById_WithRevision_ShouldReturnWorkItemAtRevision()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        // First, get the revision IDs for this work item
        var revisionsResult = await _client.GetRevisionsIdsByWorkItemIdAsync(workItemId);
        revisionsResult.IsSuccess.Should().BeTrue("Should be able to get revision IDs");
        var revisionIds = revisionsResult.Value;
        revisionIds.Should().NotBeNull();
        revisionIds.Should().NotBeEmpty("Work item should have at least one revision");

        // Get an older revision (not the latest)
        var olderRevisionId = revisionIds.Length > 1 ? revisionIds[^2] : revisionIds[^1];

        // Act - Get work item at specific revision
        var result = await _client.GetWorkItemByIdAsync(workItemId, olderRevisionId);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval at revision should succeed");
        var workItem = result.Value;
        workItem.Should().NotBeNull();
        workItem.id.Should().Be(workItemId);
        
        // Verify we can also get the latest version without revision parameter
        var latestResult = await _client.GetWorkItemByIdAsync(workItemId);
        latestResult.IsSuccess.Should().BeTrue("Latest work item retrieval should succeed");
        latestResult.Value.Should().NotBeNull();
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
            fieldList: fieldList
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
            fieldList: fieldList
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
            fieldList: fieldList
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

        var initialCount = modules.Length;

        result = await _client.GetModulesThinAsync("Archive", "DECK");
        result.Should().NotBeNull();
        modules = result.Value;
        modules.Should().NotBeNull();
        modules.Should().NotBeEmpty();

        // Assert the count is the same regardless of case
        modules.Length.Should().Be(initialCount);
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

    // The EEC Application shall set [publisher_id] according to the following mapping:<br/>
    //  <span data-source="\text{[publisher_id]} = \begin{cases}
    //  \text{[location_id]} + 14 &amp;, \hspace{0.5 cm} 1 \leq \text{[location_id]} \leq 12 \\ 
    //  0 &amp;, \hspace{0.5 cm} \text{otherwise }
    // \end{cases}" data-inline="false" class="polarion-rte-formula"></span> 

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

    [Fact]
    public void ConvertPolarionWorkItemHtmlToMarkdown_LaTeX_ShouldReturnExpectedResults()
    {
        // Arrange
        var htmlContent =
            """
            The software shall set [publisher_id] according to the following mapping:<br/>
            <span data-source="\text{[publisher_id]} = \begin{cases}
            \text{[location_id]} + 14 &amp;, \hspace{0.5 cm} 1 \leq \text{[location_id]} \leq 12 \\ 
            0 &amp;, \hspace{0.5 cm} \text{otherwise }
            \end{cases}" data-inline="false" class="polarion-rte-formula"></span>
            """;

        // Act:
        var markdownContent = _client.ConvertPolarionWorkItemHtmlToMarkdown(htmlContent);

        // Assert
        markdownContent.Should().NotBeNullOrEmpty();

        // Verify LaTeX is retained during Markdown
        markdownContent.Should().Contain("$$\\text");
        markdownContent.Should().Contain("{cases}$$");
    }

    [Fact]
    public void ConvertPolarionWorkItemHtmlToMarkdown_PolarionCrossReference_ShouldReturnExpectedResults()
    {
        // Arrange
        var htmlContent =
            """
            The software shall set the value of [epu_type] as LIFTER or TILTER according to 
            <span class="polarion-rte-link" data-type="crossReference" id="fake" data-item-id="YADA-YADA-YADA" data-option-id="longoutline">
            </span>.
            """;

        // Act:
        var markdownContent = _client.ConvertPolarionWorkItemHtmlToMarkdown(htmlContent);

        // Assert
        markdownContent.Should().NotBeNullOrEmpty();

        // Verify cross-reference is converted to a markdown link
        markdownContent.Should().Contain("[YADA-YADA-YADA](#YADA-YADA-YADA)");
    }

    [Fact]
    public async Task GetRevisionsByWorkItemIdThinAsync_ShouldReturnValid()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        // Act
        var result = await _client.GetRevisionsIdsByWorkItemIdAsync(workItemId);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var stringData = result.Value;
        stringData.Should().NotBeNull();
    }


    [Fact]
    public async Task GetWorkItemRevisionsByIdAsync_ShouldReturnValid()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        // Act (get all revisions)
        var result = await _client.GetWorkItemRevisionsByIdAsync(workItemId);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var allRevisionObjects = result.Value;
        allRevisionObjects.Should().NotBeNull();
        allRevisionObjects.Should().NotBeEmpty("Should have at least one revision");
        
        // Verify dictionary has revision IDs as keys
        allRevisionObjects.Keys.Should().AllSatisfy(key => key.Should().NotBeNullOrEmpty());
        
        // Verify all values are valid WorkItems
        allRevisionObjects.Values.Should().AllSatisfy(wi => wi.Should().NotBeNull());

        // Act (get limited revisions)
        result = await _client.GetWorkItemRevisionsByIdAsync(workItemId, 2);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var lastTwoRevisionObjects = result.Value;
        lastTwoRevisionObjects.Should().NotBeNull();
        lastTwoRevisionObjects.Count.Should().Be(2, "Should return exactly 2 revisions");

        // Verify the first two revisions from full list match the limited list
        var allRevisionsList = allRevisionObjects.Values.ToList();
        var limitedRevisionsList = lastTwoRevisionObjects.Values.ToList();
        
        allRevisionsList[0].Should().BeEquivalentTo(limitedRevisionsList[0]);
        allRevisionsList[1].Should().BeEquivalentTo(limitedRevisionsList[1]);
    }

    [Fact]
    public async Task GetRevisionsAsync_ShouldReturnValid()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        var wiResult = await _client.GetWorkItemByIdAsync(workItemId);
        wiResult.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var workItem = wiResult.Value;
        workItem.Should().NotBeNull();

        // Act (get all revisions)
        var result = await _client.GetRevisionIdsAsync(workItem.uri);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var allRevisionObjects = result.Value;
        allRevisionObjects.Should().NotBeNull();
    }


    [Fact]
    public async Task GetModuleByLocationAsync_ShouldReturnValid()
    {
        // Arrange
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;
        var misResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        misResult.IsSuccess.Should().BeTrue("Module retrieval should succeed");
        var modulesInSpace = misResult.Value;
        modulesInSpace.Should().NotBeNull();
        modulesInSpace.Should().NotBeEmpty();

        var moduleLocation = modulesInSpace[0].Location;
        moduleLocation.Should().NotBeNullOrEmpty();

        // Act
        var result = await _client.GetModuleByLocationAsync(moduleLocation);

        // Assert
        result.IsSuccess.Should().BeTrue("Module retrieval should succeed");
        var module = result.Value;
        module.Should().NotBeNull();
    }


    [Fact]
    public async Task GetModuleByUriAsync_ShouldReturnValid()
    {
        // Arrange
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;
        var misResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        misResult.IsSuccess.Should().BeTrue("Module retrieval should succeed");
        var modulesInSpace = misResult.Value;
        modulesInSpace.Should().NotBeNull();
        modulesInSpace.Should().NotBeEmpty();

        var moduleUri = modulesInSpace[0].Uri;
        moduleUri.Should().NotBeNullOrEmpty();

        // Act
        var result = await _client.GetModuleByUriAsync(moduleUri);

        // Assert
        result.IsSuccess.Should().BeTrue("Module retrieval should succeed");
        var module = result.Value;
        module.Should().NotBeNull();
    }

    // Test GetModuleRevisionsByLocationAsync
    [Fact]
    public async Task GetModuleRevisionsByLocationAsync_ShouldReturnValid()
    {
        // Arrange
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;
        var misResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        misResult.IsSuccess.Should().BeTrue("Module retrieval should succeed");
        var modulesInSpace = misResult.Value;
        modulesInSpace.Should().NotBeNull();
        modulesInSpace.Should().NotBeEmpty();

        var moduleLocation = modulesInSpace[0].Location;
        moduleLocation.Should().NotBeNullOrEmpty();

        // Act
        var result = await _client.GetModuleRevisionsByLocationAsync(moduleLocation, 2);

        // Assert
        result.IsSuccess.Should().BeTrue("Module revisions retrieval should succeed");
        var moduleRevisions = result.Value;
        moduleRevisions.Should().NotBeNull();
        moduleRevisions.Length.Should().Be(2);
    }

    #region Module/Revision API Tests

    [Fact]
    public async Task GetModuleByLocationAsync_ShouldReturnModuleWithValidUri()
    {
        // Arrange
        var moduleFolder = _config.TestScenarioData.GetModuleWorkItemUrisModuleFolder;
        var documentId = _config.TestScenarioData.GetModuleWorkItemUrisDocumentId;
        var location = $"{moduleFolder}/{documentId}";

        // Act
        var result = await _client.GetModuleByLocationAsync(location);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should find module at location '{location}'");
        var module = result.Value;
        module.Should().NotBeNull();
        module.uri.Should().NotBeNullOrEmpty("Module should have a URI");


        // Log the URI for debugging
        _output.WriteLine($"Module URI: {module.uri}");
        _output.WriteLine($"Module Name: {module.moduleName}");
    }

    [Fact]
    public async Task GetModuleWorkItemUrisAsync_WithValidModuleUri_ShouldReturnWorkItemUris()
    {
        // Arrange - First get a real module URI
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;
        var modulesResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        modulesResult.IsSuccess.Should().BeTrue();
        var moduleUri = modulesResult.Value.First().Uri;       

        _output.WriteLine($"Testing GetModuleWorkItemUrisAsync with module URI: {moduleUri}");

        // Act
        var result = await _client.GetModuleWorkItemUrisAsync(moduleUri);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should get work item URIs for module URI: {moduleUri}. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var uris = result.Value;
        uris.Should().NotBeNull();
        uris.Should().NotBeEmpty("Module should contain work items");

        // Log sample URIs
        _output.WriteLine($"Found {uris.Length} work item URIs");
        foreach (var uri in uris.Take(5))
        {
            _output.WriteLine($"Work Item URI: {uri}");
        }
    }

    [Fact]
    public async Task GetModuleWorkItemUrisAsync_WithRevision_ShouldReturnWorkItemUris()
    {
        // Arrange - Get module by location, then append revision
        var moduleFolder = _config.TestScenarioData.GetModuleWorkItemUrisModuleFolder;
        var documentId = _config.TestScenarioData.GetModuleWorkItemUrisDocumentId;
        var revision = _config.TestScenarioData.GetModuleWorkItemUrisRevision;
        var location = $"{moduleFolder}/{documentId}";

        var moduleResult = await _client.GetModuleByLocationAsync(location);
        moduleResult.IsSuccess.Should().BeTrue($"Should find module at location '{location}'");
        var moduleUri = moduleResult.Value.uri;

        // Append revision
        var moduleUriWithRevision = $"{moduleUri}%{revision}";
        _output.WriteLine($"Testing URI with revision: {moduleUriWithRevision}");

        // Act
        var result = await _client.GetModuleWorkItemUrisAsync(moduleUriWithRevision);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should get work item URIs with revision. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var uris = result.Value;
        uris.Should().NotBeNull();

        // Log results
        _output.WriteLine($"Found {uris.Length} work item URIs at revision {revision}");
        foreach (var uri in uris.Take(5))
        {
            _output.WriteLine($"Work Item URI: {uri}");
        }
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_ShouldReturnWorkItems()
    {
        // Arrange
        var moduleFolder = _config.TestScenarioData.QueryWorkItemsInModuleFolder;
        var documentId = _config.TestScenarioData.QueryWorkItemsInModuleDocumentId;

        _output.WriteLine($"Testing QueryWorkItemsInModuleAsync with folder='{moduleFolder}', documentId='{documentId}'");

        // Act
        var result = await _client.QueryWorkItemsInModuleAsync(moduleFolder, documentId);

        // Assert
        result.IsSuccess.Should().BeTrue($"SQL-based query should succeed. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var workItems = result.Value;
        workItems.Should().NotBeNull();
        workItems.Should().NotBeEmpty("Module should contain work items");

        // Verify work item properties
        _output.WriteLine($"Found {workItems.Length} work items");
        foreach (var wi in workItems.Take(5))
        {
            wi.id.Should().NotBeNullOrEmpty();
            _output.WriteLine($"WorkItem: {wi.id}, OutlineNumber: {wi.outlineNumber}, Type: {wi.type?.id}");
        }
    }

    [Fact]
    public async Task QueryWorkItemsInModuleAsync_WithTypeFilter_ShouldReturnFilteredWorkItems()
    {
        // Arrange
        var moduleFolder = _config.TestScenarioData.QueryWorkItemsInModuleFolder;
        var documentId = _config.TestScenarioData.QueryWorkItemsInModuleDocumentId;
        var itemTypes = new List<string> { "heading", "paragraph" };

        _output.WriteLine($"Testing QueryWorkItemsInModuleAsync with type filter: {string.Join(", ", itemTypes)}");

        // Act
        var result = await _client.QueryWorkItemsInModuleAsync(moduleFolder, documentId, itemTypes);

        // Assert
        result.IsSuccess.Should().BeTrue($"Filtered query should succeed. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var workItems = result.Value;
        workItems.Should().NotBeNull();

        _output.WriteLine($"Found {workItems.Length} work items with types: {string.Join(", ", itemTypes)}");

        // All returned items should be of specified types
        foreach (var wi in workItems)
        {
            itemTypes.Should().Contain(wi.type?.id, $"Work item {wi.id} has unexpected type {wi.type?.id}");
        }
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_ShouldReturnWorkItemsWithRevisionInfo()
    {
        // Arrange
        var moduleFolder = _config.TestScenarioData.GetModuleWorkItemUrisModuleFolder;
        var documentId = _config.TestScenarioData.GetModuleWorkItemUrisDocumentId;
        var revision = _config.TestScenarioData.GetModuleWorkItemUrisRevision;

        _output.WriteLine($"Testing GetWorkItemsByModuleRevisionAsync with folder='{moduleFolder}', documentId='{documentId}', revision='{revision}'");

        // Act
        var result = await _client.GetWorkItemsByModuleRevisionAsync(moduleFolder, documentId, revision);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should retrieve work items at revision {revision}. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var workItemsWithInfo = result.Value;
        workItemsWithInfo.Should().NotBeNull();
        workItemsWithInfo.Should().NotBeEmpty("Module should contain work items");

        // Verify each item has revision info
        _output.WriteLine($"Found {workItemsWithInfo.Length} work items with revision info");
        foreach (var wiInfo in workItemsWithInfo.Take(10))
        {
            wiInfo.WorkItem.Should().NotBeNull();
            wiInfo.Revision.Should().NotBeNullOrEmpty();
            wiInfo.HeadRevision.Should().NotBeNullOrEmpty();
            wiInfo.SourceUri.Should().NotBeNullOrEmpty();

            _output.WriteLine($"WorkItem: {wiInfo.WorkItem.id}, Revision: {wiInfo.Revision}, HEAD: {wiInfo.HeadRevision}, IsHistorical: {wiInfo.IsHistorical}");
        }

        // Check for historical items
        var historicalCount = workItemsWithInfo.Count(wi => wi.IsHistorical);
        var currentCount = workItemsWithInfo.Length - historicalCount;
        _output.WriteLine($"Historical: {historicalCount}, Current: {currentCount}");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_EmptyModuleFolder_ShouldFail()
    {
        // Arrange
        var moduleFolder = "";
        var documentId = "SomeDoc";
        var revision = "100";

        // Act
        var result = await _client.GetWorkItemsByModuleRevisionAsync(moduleFolder, documentId, revision);

        // Assert
        result.IsFailed.Should().BeTrue("Empty module folder should fail validation");
        _output.WriteLine($"Expected failure message: {result.Errors.First().Message}");
    }

    [Fact]
    public async Task GetWorkItemsByModuleRevisionAsync_InvalidModule_ShouldFailGracefully()
    {
        // Arrange
        var moduleFolder = "NonExistent_Folder";
        var documentId = "NonExistent_Document";
        var revision = "100";

        // Act
        var result = await _client.GetWorkItemsByModuleRevisionAsync(moduleFolder, documentId, revision);

        // Assert
        result.IsFailed.Should().BeTrue("Non-existent module should fail gracefully");
        result.Errors.First().Message.Should().Contain("Failed to get module", "Error should mention module retrieval failure");
        _output.WriteLine($"Expected failure message: {result.Errors.First().Message}");
    }

    [Fact]
    public async Task GetWorkItemByUriAsync_WithValidUri_ShouldReturnWorkItem()
    {
        // Arrange - First get a work item to get its URI
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;
        var wiResult = await _client.GetWorkItemByIdAsync(workItemId);
        wiResult.IsSuccess.Should().BeTrue();
        var workItemUri = wiResult.Value.uri;

        _output.WriteLine($"Testing GetWorkItemByUriAsync with URI: {workItemUri}");

        // Act
        var result = await _client.GetWorkItemByUriAsync(workItemUri);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should retrieve work item by URI. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var workItem = result.Value;
        workItem.Should().NotBeNull();
        workItem.id.Should().Be(workItemId);

        _output.WriteLine($"Retrieved work item: {workItem.id}, title: {workItem.title}");
    }

    [Fact]
    public async Task GetWorkItemByUriAsync_WithRevision_ShouldReturnHistoricalWorkItem()
    {
        // Arrange - Get work item and its revisions
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;
        var wiResult = await _client.GetWorkItemByIdAsync(workItemId);
        wiResult.IsSuccess.Should().BeTrue();
        var workItemUri = wiResult.Value.uri;

        // Get revision IDs
        var revisionsResult = await _client.GetRevisionIdsAsync(workItemUri);
        revisionsResult.IsSuccess.Should().BeTrue();
        var revisions = revisionsResult.Value;
        revisions.Should().NotBeEmpty();

        // Use an older revision (first in array is oldest)
        var olderRevision = revisions.Length > 1 ? revisions[0] : revisions[^1];
        var uriWithRevision = $"{workItemUri}%{olderRevision}";

        _output.WriteLine($"Testing GetWorkItemByUriAsync with historical URI: {uriWithRevision}");

        // Act
        var result = await _client.GetWorkItemByUriAsync(uriWithRevision);

        // Assert
        result.IsSuccess.Should().BeTrue($"Should retrieve historical work item by URI with revision. Error: {(result.IsFailed ? result.Errors.First().Message : "")}");
        var workItem = result.Value;
        workItem.Should().NotBeNull();

        _output.WriteLine($"Retrieved historical work item: {workItem.id} at revision {olderRevision}");
    }

    #endregion

    #region Diagnostic/Investigation Tests

    [Fact]
    public async Task Diagnostic_LogAllModuleUriFormats()
    {
        // This test logs various module URI formats to help debug URI format issues
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;

        // Get modules in space
        var modulesResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        modulesResult.IsSuccess.Should().BeTrue();

        _output.WriteLine("=== Module URIs in Space ===");
        foreach (var module in modulesResult.Value.Take(5))
        {
            _output.WriteLine($"Title: {module.Title}");
            _output.WriteLine($"  Location: {module.Location}");
            _output.WriteLine($"  URI: {module.Uri}");
            _output.WriteLine($"  Space: {module.Space}");
            _output.WriteLine("");
        }

        // Get full module details for one
        if (modulesResult.Value.Length > 0)
        {
            var firstModule = modulesResult.Value[0];
            var fullModuleResult = await _client.GetModuleByUriAsync(firstModule.Uri);
            if (fullModuleResult.IsSuccess)
            {
                var module = fullModuleResult.Value;
                _output.WriteLine("=== Full Module Details ===");
                _output.WriteLine($"URI: {module.uri}");
                _output.WriteLine($"moduleFolder: {module.moduleFolder}");
                _output.WriteLine($"moduleName: {module.moduleName}");
                _output.WriteLine($"moduleLocation: {module.moduleLocation}");
            }
        }
    }

    [Fact]
    public async Task Diagnostic_CompareUriFormats()
    {
        // Compare URI from GetModuleByLocationAsync vs manually built URI
        var moduleFolder = _config.TestScenarioData.GetModuleWorkItemUrisModuleFolder;
        var documentId = _config.TestScenarioData.GetModuleWorkItemUrisDocumentId;
        var revision = _config.TestScenarioData.GetModuleWorkItemUrisRevision;
        var projectName = _config.PolarionClient.ProjectId;

        // Get actual module
        var location = $"{moduleFolder}/{documentId}";
        var moduleResult = await _client.GetModuleByLocationAsync(location);

        if (moduleResult.IsSuccess)
        {
            var actualUri = moduleResult.Value.uri;
            _output.WriteLine($"Actual Module URI: {actualUri}");

            // Build URI using our function
            var builtUri = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);
            _output.WriteLine($"Built Module URI: {builtUri}");

            // Compare base URI (without revision)
            var builtUriWithoutRevision = builtUri.Replace($"%{revision}", "");
            _output.WriteLine($"Built URI without revision: {builtUriWithoutRevision}");
            _output.WriteLine($"URIs Match: {actualUri == builtUriWithoutRevision}");

            // Analyze URI components
            _output.WriteLine("\n=== URI Component Analysis ===");
            _output.WriteLine($"Actual URI split by '/':");
            foreach (var part in actualUri.Split('/'))
            {
                _output.WriteLine($"  '{part}'");
            }

            _output.WriteLine($"\nActual URI split by '$':");
            foreach (var part in actualUri.Split('$'))
            {
                _output.WriteLine($"  '{part}'");
            }

            _output.WriteLine($"\nActual URI split by '#':");
            foreach (var part in actualUri.Split('#'))
            {
                _output.WriteLine($"  '{part}'");
            }
        }
        else
        {
            _output.WriteLine($"Failed to get module: {moduleResult.Errors.First().Message}");
        }
    }

    [Fact]
    public async Task Diagnostic_InvestigateActualModuleUriFormat()
    {
        // This test helps understand the actual Polarion URI format
        // by fetching a real module and comparing its URI structure

        // Arrange - Get a real module URI from Polarion
        var spaceName = _config.TestScenarioData.GetDocumentsInSpaceSpaceName;
        var modulesResult = await _client.GetModulesInSpaceThinAsync(spaceName);
        modulesResult.IsSuccess.Should().BeTrue();

        var actualModuleUri = modulesResult.Value.First().Uri;

        // Log the actual URI format for analysis
        _output.WriteLine($"Actual Module URI: {actualModuleUri}");

        // Extract components from actual URI
        var parts = actualModuleUri.Split(new[] { '$', '#', '%' }, StringSplitOptions.None);
        _output.WriteLine("\nURI Parts split by $, #, %:");
        foreach (var part in parts)
        {
            _output.WriteLine($"  Part: '{part}'");
        }

        // Assert - Document actual format
        actualModuleUri.Should().StartWith("subterra:data-service:objects:");

        // Log extracted values using PolarionUriParser
        _output.WriteLine("\n=== PolarionUriParser Results ===");
        _output.WriteLine($"Extracted ID: {PolarionUriParser.ExtractIdFromUri(actualModuleUri)}");
        _output.WriteLine($"Extracted Revision: {PolarionUriParser.ExtractRevisionFromUri(actualModuleUri)}");
    }

    #endregion

}
