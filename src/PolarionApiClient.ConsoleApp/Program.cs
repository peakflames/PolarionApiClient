using System.Text.Json;
using PolarionApiClient.ConsoleApp;
using PolarionApiClient.Core;

if (args.Length < 1)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: Configuration file path is required as first argument.");
    Console.WriteLine("Usage: program.exe path/to/config.json");
    return;
}

string configFilePath = args[0];
AppConfiguration? appConfig = null;

if (!File.Exists(configFilePath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: Configuration file not found at {configFilePath}");
    return;
}


// Load and parse the configuration file
string jsonContent = File.ReadAllText(configFilePath);
appConfig = JsonSerializer.Deserialize<AppConfiguration>(jsonContent,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (appConfig == null)
{
    Console.WriteLine("Error: Failed to parse configuration file.");
    return;
}

// Now you can use the configurations as needed
var polarionConfig = appConfig.PolarionClient;
var testConfig = appConfig.TestConfig;

Console.WriteLine($"Loaded Test configuration: Project ID = {polarionConfig.ProjectId}");


try
{
    Console.WriteLine("\nStarting Polarion API test...");

    Console.WriteLine("\n📡 Authenticating with Polarion...");
    var client = await PolarionClient.CreateAsync(polarionConfig);
    if (client is null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("❌ Failed to create Polarion client");
        return;
    }


    Console.WriteLine("\n🧪 Testing GetWorkItemByIdAsync()...");
    var workItemResult = await client.GetWorkItemByIdAsync(testConfig.GetWorkItemByIdAsyncWorkItemId);
    if(workItemResult.IsFailed)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"❌ Failed to get work item: {testConfig.GetWorkItemByIdAsyncWorkItemId}");
        return;
    }
    
    var workItem = workItemResult.Value;
    
    if (workItem != null)
    {
        Console.WriteLine($"✅ Found work item: {workItem.id} - {workItem.title}");
        Console.WriteLine($"\tType: {workItem.type.id}, Status: {workItem.status.id}");
        Console.WriteLine($"\tCreated: {workItem.created}, Updated: {workItem.updated}");
        Console.WriteLine($"\tAuthor: {workItem.author.name}");
        Console.WriteLine($"\tDescription: {workItem.description.content.Take(50)}...");
    }
    else
    {
        Console.WriteLine("❌ Work item not found.");
    }

    // Example: Query work items
    var title = "L5 BMU Control Application SRS";
    var documentFilter = $"document.title:\"{title}\"";

    Console.WriteLine("\n\n🧪 Testing SearchWorkitem()...");

    var searchResult = await client.SearchWorkitem(
        query: testConfig.SearchWorkitemQuery,
        order: testConfig.SearchWorkitemOrder,
        field_list: testConfig.SearchWorkitemFieldList
    );

    if (searchResult.IsFailed)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"❌ Failed to search for workitems: {searchResult.Errors.FirstOrDefault()}");
        return;
    }

    var workItems = searchResult.Value;

    Console.WriteLine($"✅ Found {workItems.Length} items. Listing first 5:");
    
    foreach (var item in workItems.Take(5))
    {
        Console.WriteLine($"  - {item.id}: {item.title} ({item.type.id})");
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n✅ Completed.\n\n");
}
catch (Exception ex)
{
    var currentColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"❌ Inner error: {ex.InnerException.Message}");
    }
    Console.ForegroundColor = currentColor;
}


