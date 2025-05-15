using System.Diagnostics.CodeAnalysis;
using Polarion;

[RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
public class Program
{
    [RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Polarion Trimming Test");
        
        // Create a mock configuration (we won't actually connect to a server)
        var config = new PolarionClientConfiguration("", "", "", "");

        try
        {
            // Test all four methods
            await TestGetWorkItemByIdAsync(config);
            await TestSearchWorkitemAsync(config);
            await TestSearchWorkitemInBaselineAsync(config);
            await TestGetDocumentsInSpaceAsync(config);
            
            Console.WriteLine("All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    [RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
    private static async Task TestGetWorkItemByIdAsync(PolarionClientConfiguration config)
    {
        Console.WriteLine("Testing GetWorkItemByIdAsync...");
        try
        {
            var client = await PolarionClient.CreateAsync(config);
            if (client.IsSuccess)
            {
                var result = await client.Value.GetWorkItemByIdAsync("TEST-123");
                Console.WriteLine($"GetWorkItemByIdAsync completed with status: {(result.IsSuccess ? "Success" : "Failure")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetWorkItemByIdAsync test failed: {ex.Message}");
            throw;
        }
    }

    [RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
    private static async Task TestSearchWorkitemAsync(PolarionClientConfiguration config)
    {
        Console.WriteLine("Testing SearchWorkitemAsync...");
        try
        {
            var client = await PolarionClient.CreateAsync(config);
            if (client.IsSuccess)
            {
                var result = await client.Value.SearchWorkitemAsync("type:requirement", "Created", new List<string> { "id" });
                Console.WriteLine($"SearchWorkitemAsync completed with status: {(result.IsSuccess ? "Success" : "Failure")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SearchWorkitemAsync test failed: {ex.Message}");
            throw;
        }
    }

    [RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
    private static async Task TestSearchWorkitemInBaselineAsync(PolarionClientConfiguration config)
    {
        Console.WriteLine("Testing SearchWorkitemInBaselineAsync...");
        try
        {
            var client = await PolarionClient.CreateAsync(config);
            if (client.IsSuccess)
            {
                var result = await client.Value.SearchWorkitemInBaselineAsync("baseline1", "type:requirement", "Created", new List<string> { "id" });
                Console.WriteLine($"SearchWorkitemInBaselineAsync completed with status: {(result.IsSuccess ? "Success" : "Failure")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SearchWorkitemInBaselineAsync test failed: {ex.Message}");
            throw;
        }
    }

    [RequiresUnreferencedCode("Uses Polarion API which requires reflection")]
    private static async Task TestGetDocumentsInSpaceAsync(PolarionClientConfiguration config)
    {
        Console.WriteLine("Testing GetDocumentsInSpaceAsync...");
        try
        {
            var client = await PolarionClient.CreateAsync(config);
            if (client.IsSuccess)
            {
                var result = await client.Value.GetModulesInSpaceThinAsync("TestSpace");
                Console.WriteLine($"GetDocumentsInSpaceAsync completed with status: {(result.IsSuccess ? "Success" : "Failure")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetDocumentsInSpaceAsync test failed: {ex.Message}");
            throw;
        }
    }
}
