namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Default fields for <see cref="QueryWorkItemsInModuleAsync"/>.
    /// </summary>
    private static readonly List<string> SqlQueryWorkItemFields =
    [
        "id", "type", "title", "description", "status", "outlineNumber"
    ];

    /// <summary>
    /// Gets the work items of a document (module) at HEAD, in document order.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    ///   1. Build the module URI from <c>moduleFolder</c> and <c>documentId</c> (no server round trip)
    ///   2. Call <see cref="GetModuleWorkItemsAsync"/> on that URI
    ///   3. Drop unresolvable rows, then apply the optional <paramref name="itemTypes"/> filter client-side
    ///
    /// Pinned references are returned with the values at their pinned revision, and pinned references
    /// to items deleted after pinning are included. Use <see cref="GetModuleWorkItemsAsync"/> directly
    /// to see unresolvable rows or the per-item pinned revision.
    ///
    /// Behavior change (previously a SQL query on POLARION.REL_MODULE_WORKITEM): results are always in
    /// document order and <paramref name="sort"/> is ignored; a document with no matching items returns
    /// a successful, empty array instead of a failure. A location with no document at HEAD still fails
    /// (Polarion raises an unresolvable-object error for the module URI).
    /// </remarks>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="itemTypes">Optional list of work item type IDs to keep</param>
    /// <param name="sort">Ignored. Kept for source compatibility; results are always in document order</param>
    /// <param name="fields">Optional list of fields to retrieve. "type" is added automatically when
    /// <paramref name="itemTypes"/> is set</param>
    /// <returns>Array of work items in the module, in document order</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> QueryWorkItemsInModuleAsync(
        string moduleFolder,
        string documentId,
        List<string>? itemTypes = null,
        string sort = "outlineNumber",
        List<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(moduleFolder))
        {
            return Result.Fail("Module folder cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(documentId))
        {
            return Result.Fail("Document ID cannot be null or empty");
        }

        var filterTypes = itemTypes is { Count: > 0 } ? new HashSet<string>(itemTypes, StringComparer.Ordinal) : null;

        // Copy so the caller's list (or the shared default) is never mutated
        var fieldList = new List<string>(fields ?? SqlQueryWorkItemFields);
        if (filterTypes is not null && !fieldList.Contains("type"))
        {
            fieldList.Add("type");
        }

        var rowsResult = await GetModuleWorkItemsAsync(BuildModuleUri(moduleFolder, documentId), null, true, fieldList);
        if (rowsResult.IsFailed)
        {
            return Result.Fail<WorkItem[]>(
                $"Failed to get work items in document '{moduleFolder}/{documentId}': {rowsResult.Errors.First().Message}");
        }

        var workItems = rowsResult.Value
            .Where(row => !row.IsUnresolvable)
            .Select(row => row.WorkItem)
            .Where(wi => filterTypes is null || (wi.type?.id is { } typeId && filterTypes.Contains(typeId)))
            .ToArray();

        return Result.Ok(workItems);
    }

    /// <summary>
    /// Builds the module URI for <c>moduleFolder/documentId</c> without a server round trip.
    /// </summary>
    /// <remarks>
    /// <c>getModuleByLocation</c> returns the whole <see cref="Module"/>, which for a branched document
    /// embeds the parent document at the branch revision (megabytes). The URI has a fixed form, so it is
    /// built here instead. A location with no document makes <c>getModuleWorkItems</c> fail with an
    /// unresolvable-object error at HEAD and at a revision, so a missing document still fails.
    /// </remarks>
    private string BuildModuleUri(string moduleFolder, string documentId) =>
        PolarionUriParser.BuildModuleUri(_config.ProjectId, moduleFolder, documentId);
}
