namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets a work item by its URI (the URI may include a revision specifier).
    /// </summary>
    /// <param name="uri">The Polarion work item URI</param>
    /// <returns>The work item at the URI's embedded revision</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem>> GetWorkItemByUriAsync(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return Result.Fail("Work item URI cannot be null or empty");
        }

        try
        {
            var request = new getWorkItemByUriRequest(uri);
            var response = await _trackerClient.getWorkItemByUriAsync(request);

            if (response?.getWorkItemByUriReturn is null)
            {
                return Result.Fail($"No work item found for URI '{uri}'");
            }

            return Result.Ok(response.getWorkItemByUriReturn);
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get work item by URI '{uri}'", ex);
        }
    }

    /// <summary>
    /// Gets a work item at a specific revision using its URI.
    /// </summary>
    /// <param name="uri">The Polarion work item URI</param>
    /// <param name="revision">The revision to retrieve</param>
    /// <returns>The work item at the specified revision</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem>> GetWorkItemByUriInRevisionAsync(string uri, string revision)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return Result.Fail("Work item URI cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(revision))
        {
            return Result.Fail("Revision cannot be null or empty");
        }

        try
        {
            var request = new getWorkItemByUriInRevisionRequest(uri, revision);
            var response = await _trackerClient.getWorkItemByUriInRevisionAsync(request);

            if (response?.getWorkItemByUriInRevisionReturn is null)
            {
                return Result.Fail($"No work item found for URI '{uri}' at revision '{revision}'");
            }

            return Result.Ok(response.getWorkItemByUriInRevisionReturn);
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get work item by URI '{uri}' at revision '{revision}'", ex);
        }
    }
}
