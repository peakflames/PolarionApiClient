namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get a work item by its ID, optionally at a specific revision.
    /// </summary>
    /// <param name="workItemId">The work item ID</param>
    /// <param name="revision">Optional revision ID. If null, returns the latest version.</param>
    /// <returns>The work item at the specified revision or latest if revision is null</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId, string? revision = null)
    {
        try
        {
            // When no revision specified, use the fast path (single API call)
            if (revision == null)
            {
                var result = await _trackerClient.getWorkItemByIdAsync(new(_config.ProjectId, workItemId));
                return result is null ? Result.Fail("Work item not found") : result.getWorkItemByIdReturn;
            }
            
            // When revision specified, we need the URI first
            var workItemResult = await _trackerClient.getWorkItemByIdAsync(new(_config.ProjectId, workItemId));
            
            if (workItemResult is null)
            {
                return Result.Fail("Work item not found");
            }
            
            var workItem = workItemResult.getWorkItemByIdReturn;
            
            // Now get the specific revision using the URI
            var revisionResult = await _trackerClient.getWorkItemByUriInRevisionAsync(new(workItem.uri, revision));
            
            return revisionResult is null
                ? Result.Fail($"Revision {revision} for work item {workItemId} not found")
                : revisionResult.getWorkItemByUriInRevisionReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException(
                $"Failed to get work item {workItemId}" + 
                (revision != null ? $" at revision {revision}" : ""), ex);
        }
    }    
}
