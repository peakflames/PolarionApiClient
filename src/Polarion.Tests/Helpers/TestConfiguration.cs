using System.Text.Json;
using Polarion;

namespace Polarion.Tests.Helpers;

public class TestConfigurationLoader
{
    public PolarionClientConfiguration PolarionConfig { get; set; } = null!;
    public TestScenarioData TestScenarioData { get; set; } = null!;

    public static TestConfiguration Load(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found at {configPath}");
        }

        string jsonContent = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<TestConfiguration>(jsonContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (config is null)
        {
            throw new InvalidOperationException("Failed to parse configuration file");
        }

        return config;
    }
}

public class TestConfiguration
{
    public required PolarionClientConfiguration PolarionClient { get; set; }
    public required TestScenarioData TestScenarioData { get; set; }
}

public class TestScenarioData
{
    public string GetWorkItemByIdAsyncWorkItemId { get; set; } = null!;
    public string SearchWorkitemQuery { get; set; } = null!;
    public string SearchWorkitemOrder { get; set; } = null!;
    public string[] SearchWorkitemFieldList { get; set; } = null!;
    public string SearchWorkitemBaselineRevision { get; set; } = null!;
    public string GetDocumentsInSpaceSpaceName { get; set; } = null!;
    public string GetWorktemsByModuleModuleTitle { get; set; } = null!;
    public string GetWorktemsByModuleModuleWorkItemFilter { get; set; } = null!;
    public string[] GetWorktemsByModuleModuleWorkItemCustomFieldList { get; set; } = null!;
    public string GetHierarchicalWorkItemsByModuleItemPrefix { get; set; } = null!;
    public Dictionary<string, string> ExportModuleToMarkdownWorkItemTypeToShortNameMap { get; set; } = null!;
    
    // Module/Revision API test data
    public string GetModuleWorkItemUrisModuleFolder { get; set; } = null!;
    public string GetModuleWorkItemUrisDocumentId { get; set; } = null!;
    public string GetModuleWorkItemUrisRevision { get; set; } = null!;
    public string QueryWorkItemsInModuleFolder { get; set; } = null!;
    public string QueryWorkItemsInModuleDocumentId { get; set; } = null!;
}
