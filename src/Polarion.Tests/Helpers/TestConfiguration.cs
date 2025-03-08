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
}
