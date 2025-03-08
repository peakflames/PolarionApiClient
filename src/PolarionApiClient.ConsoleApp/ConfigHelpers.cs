using PolarionApiClient.Core;

namespace PolarionApiClient.ConsoleApp;

internal class AppConfiguration
{
    public required PolarionClientConfiguration PolarionClient { get; set; }
    public required TestConfiguration TestConfig { get; set; }
}

internal class TestConfiguration
{
    public required string GetWorkItemByIdAsyncWorkItemId { get; set; }
    public required string SearchWorkitemQuery { get; set; }
    public required string SearchWorkitemOrder { get; set; }
    public List<string> SearchWorkitemFieldList { get; set; } = [];
}