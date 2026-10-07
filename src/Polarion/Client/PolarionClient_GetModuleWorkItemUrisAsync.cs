namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets URIs of all work items in a module at the specified revision.
    /// </summary>
    /// <param name="moduleUri">The module URI (may include revision specifier)</param>
    /// <param name="parentWorkItemUri">Optional parent work item URI to filter children</param>
    /// <param name="deep">Whether to include external/linked items</param>
    /// <returns>Array of work item URIs</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<string[]>> GetModuleWorkItemUrisAsync(string moduleUri, string? parentWorkItemUri = null, bool deep = true)
    {
        if (string.IsNullOrWhiteSpace(moduleUri))
        {
            return Result.Fail("Module URI cannot be null or empty");
        }

        try
        {
            var request = new getModuleWorkItemUrisRequest(moduleUri, parentWorkItemUri, deep);
            var response = await _trackerClient.getModuleWorkItemUrisAsync(request);

            // An empty document comes back with a null array; that is a valid, empty result.
            return Result.Ok(response?.getModuleWorkItemUrisReturn ?? []);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error($"Failed to get module work item URIs for '{moduleUri}'. Exception: {ex.Message}"));
        }
    }
}
