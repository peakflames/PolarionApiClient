namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets URIs of all work items in a module at the specified revision.
    /// </summary>
    /// <param name="moduleUri">The module URI (may include revision specifier)</param>
    /// <param name="parentWorkItemUri">Optional parent work item URI to filter children</param>
    /// <param name="deep">Whether to include external/linked items</param>
    /// <returns>Array of work item URIs. An empty document returns a successful, empty array. A URI with no
    /// document behind it fails, because Polarion raises an unresolvable-object error. As a fallback, if no
    /// URIs come back and Polarion reports the module as unresolvable, the failure is "Document not
    /// found"</returns>
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

            var uris = response?.getModuleWorkItemUrisReturn;
            if (uris is { Length: > 0 })
            {
                return Result.Ok(uris);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error($"Failed to get module work item URIs for '{moduleUri}'. Exception: {ex.Message}"));
        }

        // No rows: an empty document is a valid, empty result; a URI with no document behind it is not.
        var exists = await EnsureModuleResolvableAsync(moduleUri, $"Document not found: '{moduleUri}'");
        return exists.IsFailed ? Result.Fail<string[]>(exists.Errors) : Result.Ok<string[]>([]);
    }
}
