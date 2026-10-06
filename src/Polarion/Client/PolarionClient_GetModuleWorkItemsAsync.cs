namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets the work items of a document (module) in document order, including referenced items.
    /// </summary>
    /// <param name="moduleUri">The module URI, optionally with a revision suffix (<c>moduleUri%revision</c>)
    /// to read the document as it was at that revision or at a baseline's base revision</param>
    /// <param name="parentWorkItemUri">Optional parent work item URI; when set, only its children are returned</param>
    /// <param name="deep">When true (default), returns the whole tree below the parent (or the whole document);
    /// when false, only direct children</param>
    /// <param name="fields">Optional list of fields to retrieve. Defaults to id, type, title, description,
    /// status, outlineNumber, author, created and updated</param>
    /// <returns>Result containing one <see cref="ModuleWorkItem"/> per document row, in document order.
    /// An empty document returns a successful, empty array</returns>
    /// <exception cref="PolarionClientException">Not thrown; service errors are returned as a failed result</exception>
    /// <remarks>
    /// Wraps the Polarion SOAP <c>getModuleWorkItems</c> call. Compared with SQL or Lucene queries, it:
    /// <list type="bullet">
    /// <item>returns pinned references with the field values at their pinned revision, and exposes that
    /// revision in <see cref="ModuleWorkItem.Revision"/>;</item>
    /// <item>returns pinned references to items deleted after they were pinned;</item>
    /// <item>returns rows in document order (do not re-sort by <c>outlineNumber</c>: referenced items carry
    /// the outline number of their home document);</item>
    /// <item>returns unresolvable rows (e.g. live references to deleted items) flagged with
    /// <see cref="ModuleWorkItem.IsUnresolvable"/>. Callers decide whether to skip or report them.</item>
    /// </list>
    /// It is a single SOAP call; the response size grows with the document size and the field list.
    /// </remarks>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<ModuleWorkItem[]>> GetModuleWorkItemsAsync(
        string moduleUri,
        string? parentWorkItemUri = null,
        bool deep = true,
        List<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(moduleUri))
        {
            return Result.Fail("Module URI cannot be null or empty");
        }

        string[] fieldArray = [.. fields ?? DefaultWorkItemFields];

        try
        {
            var response = await _trackerClient.getModuleWorkItemsAsync(
                new getModuleWorkItemsRequest(moduleUri, parentWorkItemUri!, deep, fieldArray));

            var rows = response?.getModuleWorkItemsReturn;
            if (rows is null || rows.Length == 0)
            {
                return Result.Ok(Array.Empty<ModuleWorkItem>());
            }

            var items = rows
                .Where(row => row is not null)
                .Select(ToModuleWorkItem)
                .ToArray();

            return Result.Ok(items);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get work items for module '{moduleUri}'. {ex.Message}");
        }
    }

    private static ModuleWorkItem ToModuleWorkItem(WorkItem row)
    {
        var uri = row.uri ?? string.Empty;
        var id = !string.IsNullOrEmpty(row.id) ? row.id : PolarionUriParser.ExtractIdFromUri(uri);
        var revision = uri.Contains('%') ? PolarionUriParser.ExtractRevisionFromUri(uri) : string.Empty;

        return new ModuleWorkItem
        {
            WorkItem = row,
            Uri = uri,
            Id = id,
            Revision = revision,
            IsUnresolvable = row.unresolvable,
        };
    }
}
