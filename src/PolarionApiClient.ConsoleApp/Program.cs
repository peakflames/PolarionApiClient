using System.ServiceModel;
using PolarionApiClient.Core;
using PolarionApiClient.Core.Generated;
using PolarionApiClient.Core.Models;

var config = new PolarionClientConfiguration(){
    ServerUrl = "https://polarion-url/",
    Username = "my-username",
    Password = "my-password",
    ProjectId = "my-project-id",
    TimeoutSeconds = 60,
};


try
{
    Console.WriteLine("Starting Polarion API test...");

    var workItemId = "MD-81294";

    var client = await PolarionClient.CreateAsync(config);
    if (client is null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("Failed to create Polarion client");
        return;
    }

    var workItemResult = await client.GetWorkItemByIdAsync(workItemId);
    if(workItemResult.IsFailed)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Failed to get work item: {workItemId}");
        return;
    }
    
    var workItem = workItemResult.Value;
    
    if (workItem != null)
    {
        Console.WriteLine($"Found work item: {workItem.id} - {workItem.title}");
        Console.WriteLine($"Type: {workItem.type}, Status: {workItem.status}");
        Console.WriteLine($"Created: {workItem.created}, Updated: {workItem.updated}");
        Console.WriteLine($"Author: {workItem.author}");
        Console.WriteLine($"Description: {workItem.description}");
    }
    else
    {
        Console.WriteLine("Work item not found.");
    }

    // // Example: Query work items
    // Console.WriteLine("\nEnter a query (e.g. 'type:requirement'):");
    // var query = Console.ReadLine();

    // var workItems = await client.QueryWorkItemsAsync(query);
    // Console.WriteLine($"Found {workItems.Length} items:");
    
    // foreach (var item in workItems)
    // {
    //     Console.WriteLine($"  - {item.Id}: {item.Title} ({item.Type})");
    // }
}
catch (Exception ex)
{
    var currentColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
    }
    Console.ForegroundColor = currentColor;
}


