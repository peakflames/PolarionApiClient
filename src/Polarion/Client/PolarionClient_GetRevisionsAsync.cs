namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get revision ids for any persistent item by its uri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns>Array of revision ids</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<string[]>> GetRevisionIdsAsync(string uri)
    {
        try
        {
            var revisionsResult = await _trackerClient.getRevisionsAsync(new getRevisionsRequest(uri));
            return revisionsResult is null
                ? Result.Fail($"No revisions found for workitem '{uri}'")
                : revisionsResult.getRevisionsReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get revisions for uri {uri}", ex);
        }
    }

    
}
