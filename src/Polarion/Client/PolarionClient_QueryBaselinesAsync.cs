namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Queries baselines (project and document baselines) with a Lucene query, scoped to the
    /// configured project by default.
    /// </summary>
    /// <param name="query">Lucene query over baselines, passed to Polarion unchanged</param>
    /// <param name="sort">Sort field (default: baseRevision)</param>
    /// <param name="includeAllProjects">When false (default), only baselines taken on the configured project
    /// or on one of its documents are returned. When true, baselines of every project are returned.</param>
    /// <returns>Result containing the matching baselines. A query that matches nothing returns a successful,
    /// empty array</returns>
    /// <exception cref="PolarionClientException">Not thrown; service errors are returned as a failed result</exception>
    /// <remarks>
    /// Wraps the Polarion SOAP <c>queryBaselines</c> call. Baseline IDs and names are not unique across
    /// projects, so an unscoped query can match baselines in several projects.
    /// <para>
    /// Scoping is applied to the results, not added to the Lucene query: a baseline is kept when its
    /// <c>baseObjectURI</c> (the project or module it was taken on) belongs to the configured project, i.e.
    /// starts with <c>subterra:data-service:objects:/default/{ProjectId}$</c>.
    /// </para>
    /// Each <see cref="Baseline"/> carries its <c>id</c>, <c>name</c>, <c>baseRevision</c> and
    /// <c>baseObjectURI</c>. To read a document as it was at a baseline, pass
    /// <c>{moduleUri}%{baseRevision}</c> to <see cref="GetModuleWorkItemsAsync"/>.
    /// </remarks>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Baseline[]>> QueryBaselinesAsync(string query, string sort = "baseRevision", bool includeAllProjects = false)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result.Fail("Query cannot be null or empty");
        }

        try
        {
            var response = await _trackerClient.queryBaselinesAsync(
                new queryBaselinesRequest(query, string.IsNullOrWhiteSpace(sort) ? "baseRevision" : sort));
            var baselines = (response?.queryBaselinesReturn ?? []).Where(b => b is not null);

            if (!includeAllProjects)
            {
                var projectPrefix = $"subterra:data-service:objects:/default/{_config.ProjectId}$";
                baselines = baselines.Where(b => b.baseObjectURI?.StartsWith(projectPrefix, StringComparison.Ordinal) == true);
            }

            return Result.Ok(baselines.ToArray());
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to query baselines. {ex.Message}");
        }
    }
}
