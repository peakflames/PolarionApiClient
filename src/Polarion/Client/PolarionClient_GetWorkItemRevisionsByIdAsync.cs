namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get revisions for a work item by its ID.
    /// </summary>
    /// <param name="workItemId">The target workitem</param>
    /// <param name="maxRevisions">Max number of revisions to return newest to oldest. -1 returns all</param>
    /// <returns>List of Revision objects</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> GetWorkItemRevisionsByIdAsync(string workItemId, int maxRevisions = -1)
    {
        try
        {
            var wiResult = await GetWorkItemByIdAsync(workItemId);
            if (wiResult.IsFailed)
            {
                return Result.Fail(wiResult.Errors);
            }

            var workItem = wiResult.Value;
            var revisionsResult = await _trackerClient.getRevisionsAsync(new getRevisionsRequest(workItem.uri));
            if (revisionsResult is null)
            {
                return Result.Fail($"No revisions found for workitem '{workItemId}'");
            } 
             ;

            var revisionIds = revisionsResult.getRevisionsReturn;

            var workItemRevisions = new List<WorkItem>();

            // Get revisions in reverse order (latest first)
            foreach (var i in Enumerable.Range(0, revisionIds.Length).Reverse())
            {
                var revisionId = revisionIds[i];
                var revisionResult = await _trackerClient.getWorkItemByUriInRevisionAsync(new (workItem.uri, revisionId));
                if (revisionResult is null)
                {
                    return Result.Fail($"Revision {revisionId} for workitem {workItemId} not found");
                }

                workItemRevisions.Add(revisionResult.getWorkItemByUriInRevisionReturn);

                // Stop if we've reached the maximum number of revisions requested
                if (maxRevisions != -1 && workItemRevisions.Count >= maxRevisions)
                {
                    break;
                }
            }

            return workItemRevisions.ToArray();
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get revisions for work item {workItemId}", ex);
        }
    }

    
}
