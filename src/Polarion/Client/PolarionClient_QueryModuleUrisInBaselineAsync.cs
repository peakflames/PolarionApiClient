namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Queries the URIs of documents (modules) as they existed at a baseline revision.
    /// </summary>
    /// <param name="baselineRevision">The baseline's base revision</param>
    /// <param name="query">Lucene query over modules, passed to Polarion unchanged. It is not scoped to the
    /// configured project automatically</param>
    /// <param name="sort">Sort field (default: uri)</param>
    /// <param name="limit">Maximum number of results (-1 = all)</param>
    /// <returns>Result containing the module URIs. A query that matches nothing returns a successful, empty array</returns>
    /// <exception cref="PolarionClientException">Not thrown; service errors are returned as a failed result</exception>
    /// <remarks>
    /// Wraps the Polarion SOAP <c>queryModuleUrisInBaseline</c> call. The returned URIs can be suffixed with
    /// <c>%{baselineRevision}</c> and passed to <see cref="GetModuleWorkItemsAsync"/>.
    /// </remarks>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<string[]>> QueryModuleUrisInBaselineAsync(
        string baselineRevision,
        string query,
        string sort = "uri",
        int limit = -1)
    {
        if (string.IsNullOrWhiteSpace(baselineRevision))
        {
            return Result.Fail("Baseline revision cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Result.Fail("Query cannot be null or empty");
        }

        try
        {
            var response = await _trackerClient.queryModuleUrisInBaselineAsync(
                new queryModuleUrisInBaselineRequest(query, sort, baselineRevision, limit));
            return Result.Ok(response?.queryModuleUrisInBaselineReturn ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to query module URIs in baseline {baselineRevision}. {ex.Message}");
        }
    }
}
