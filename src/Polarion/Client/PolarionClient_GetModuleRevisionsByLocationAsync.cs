namespace Polarion;

public partial class PolarionClient : IPolarionClient
{

    /// <summary>
    /// Get module revisions by location
    /// </summary>
    /// <remarks>
    /// Get retrieve module revisions by location with configurable
    /// maximum revision limit. Includes integration test to verify the method
    /// returns the correct number of revisions for a given module location.
    /// </remarks>
    /// <param name="location">The path of the module (e.g., MySpace/MyDoc)</param>
    /// <param name="maxRevisions">Max number of revisions to return newest to oldest. -1 returns all</param>
    /// <returns>Array Modules</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Module[]>> GetModuleRevisionsByLocationAsync(string location, int maxRevisions = -1)
    {
        try
        {
            var moduleResult = await GetModuleByLocationAsync(location);
            if (moduleResult.IsFailed)
            {
                return Result.Fail(moduleResult.Errors);
            }

            var module = moduleResult.Value;
            var revisionsResult = await _trackerClient.getRevisionsAsync(new getRevisionsRequest(module.uri));
            if (revisionsResult is null)
            {
                return Result.Fail($"No revisions found for module at location '{location}'");
            }
             ;

            var revisionIds = revisionsResult.getRevisionsReturn;

            var moduleRevisions = new List<Module>();

            // Get revisions in reverse order (latest first)
            foreach (var i in Enumerable.Range(0, revisionIds.Length).Reverse())
            {
                var revisionId = revisionIds[i];
                // Use %revision format instead of ?revision= to get full metadata
                var moduleRevisionUri = $"{module.uri}%{revisionId}";
                var revisionResult = await _trackerClient.getModuleByUriAsync(new (moduleRevisionUri));
                if (revisionResult is null)
                {
                    return Result.Fail($"Revision {revisionId} for module locaiton {location} not found");
                }

                moduleRevisions.Add(revisionResult.getModuleByUriReturn);

                // Stop if we've reached the maximum number of revisions requested
                if (maxRevisions != -1 && moduleRevisions.Count >= maxRevisions)
                {
                    break;
                }
            }

            return moduleRevisions.ToArray();
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get revisions for module at locaiton '{location}'", ex);
        }
    }

    
}
