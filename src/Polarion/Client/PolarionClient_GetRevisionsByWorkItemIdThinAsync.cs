namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get revision identifiers for a work item by its ID.
    /// </summary>
    /// <param name="workItemId"></param>
    /// <returns>Array of strings</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<string[]>> GetRevisionsByWorkItemIdThinAsync(string workItemId)
    {
        try
        {
            var wiResult = await GetWorkItemByIdAsync(workItemId);
            if (wiResult.IsFailed)
            {
                return Result.Fail(wiResult.Errors);
            }

            var workItem = wiResult.Value;
            var result = await _trackerClient.getRevisionsAsync(new getRevisionsRequest(workItem.uri));
            return result is null ? Result.Fail("Work item not found") : result.getRevisionsReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get revisions for work item {workItemId}", ex);
        }
    }
    
}
