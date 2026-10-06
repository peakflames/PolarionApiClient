namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Queries baselines (project and document baselines) with a Lucene query.
    /// </summary>
    /// <param name="query">Lucene query over baselines, passed to Polarion unchanged. It is not scoped to the
    /// configured project automatically; include e.g. <c>project.id:MyProject</c> to scope it</param>
    /// <param name="sort">Sort field (default: baseRevision)</param>
    /// <returns>Result containing the matching baselines. A query that matches nothing returns a successful,
    /// empty array</returns>
    /// <exception cref="PolarionClientException">Not thrown; service errors are returned as a failed result</exception>
    /// <remarks>
    /// Wraps the Polarion SOAP <c>queryBaselines</c> call. Each <see cref="Baseline"/> carries its
    /// <c>id</c>, <c>name</c>, <c>baseRevision</c> and <c>baseObjectURI</c> (the project or the module the
    /// baseline was taken on). To read a document as it was at a baseline, pass
    /// <c>{moduleUri}%{baseRevision}</c> to <see cref="GetModuleWorkItemsAsync"/>.
    /// </remarks>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Baseline[]>> QueryBaselinesAsync(string query, string sort = "baseRevision")
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result.Fail("Query cannot be null or empty");
        }

        try
        {
            var response = await _trackerClient.queryBaselinesAsync(new queryBaselinesRequest(query, sort));
            return Result.Ok(response?.queryBaselinesReturn ?? Array.Empty<Baseline>());
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to query baselines. {ex.Message}");
        }
    }
}
