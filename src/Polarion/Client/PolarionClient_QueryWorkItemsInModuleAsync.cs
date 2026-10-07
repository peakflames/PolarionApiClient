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
    ///   1. Get the module by location (<c>moduleFolder/documentId</c>) to obtain its URI
    ///   2. Call <see cref="GetModuleWorkItemsAsync"/> on that URI
    ///   3. Drop unresolvable rows, then apply the optional <paramref name="itemTypes"/> filter client-side
    ///
    /// Pinned references are returned with the values at their pinned revision, and pinned references
    /// to items deleted after pinning are included. Use <see cref="GetModuleWorkItemsAsync"/> directly
    /// to see unresolvable rows or the per-item pinned revision.
    ///
    /// Behavior change (previously a SQL query on POLARION.REL_MODULE_WORKITEM): results are always in
    /// document order and <paramref name="sort"/> is ignored; a document with no matching items returns
    /// a successful, empty array instead of a failure. A location with no document at HEAD (Polarion
    /// returns an unresolvable module) still fails.
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

        var moduleUriResult = await GetModuleUriByLocationAsync(moduleFolder, documentId, allowUnresolvable: false);
        if (moduleUriResult.IsFailed)
        {
            return Result.Fail<WorkItem[]>(moduleUriResult.Errors);
        }

        var rowsResult = await GetModuleWorkItemsAsync(moduleUriResult.Value, null, true, fieldList);
        if (rowsResult.IsFailed)
        {
            return Result.Fail<WorkItem[]>(rowsResult.Errors);
        }

        var workItems = rowsResult.Value
            .Where(row => !row.IsUnresolvable)
            .Select(row => row.WorkItem)
            .Where(wi => filterTypes is null || (wi.type?.id is { } typeId && filterTypes.Contains(typeId)))
            .ToArray();

        return Result.Ok(workItems);
    }

    /// <summary>
    /// Resolves <c>moduleFolder/documentId</c> to the module URI.
    /// </summary>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="allowUnresolvable">
    /// Polarion may answer a lookup for a location with no document at HEAD with an unresolvable module
    /// that still carries a URI. Pass false for HEAD reads, so a missing document fails instead of
    /// reading as empty. Pass true for historical reads, where a document deleted since can still be
    /// read at an older revision through that URI.
    /// </param>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    private async Task<Result<string>> GetModuleUriByLocationAsync(string moduleFolder, string documentId, bool allowUnresolvable)
    {
        var location = $"{moduleFolder}/{documentId}";
        var moduleResult = await GetModuleByLocationAsync(location);

        if (moduleResult.IsFailed)
        {
            return Result.Fail<string>(
                $"Failed to get module at location '{location}': {moduleResult.Errors.First().Message}");
        }

        var module = moduleResult.Value;
        if (string.IsNullOrEmpty(module?.uri))
        {
            return Result.Fail<string>($"Module at location '{location}' has no URI");
        }

        if (module.unresolvable && !allowUnresolvable)
        {
            return Result.Fail<string>($"Module at location '{location}' was not found (unresolvable)");
        }

        return Result.Ok(module.uri);
    }
}
