namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Default fields to retrieve when querying work items.
    /// Covers all fields consumed by the REST API WorkItemAttributes response model.
    /// </summary>
    private static readonly List<string> DefaultWorkItemFields =
    [
        "id", "type", "title", "description", "status", "outlineNumber",
        "author", "created", "updated"
    ];

    /// <summary>
    /// Queries work items from a module at a specific historical revision.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    ///   1. Get module by location to obtain its real URI
    ///   2. Append revision to URI and get work item URIs; extract IDs from them
    ///   3. Bulk fetch work item data at the specified baseline revision via SearchWorkitemInBaselineAsync
    ///   4. Wrap results as WorkItemWithRevisionInfo (IsHistorical=true always, since this
    ///      function is only called for historical document queries)
    ///
    /// Step 3 uses SearchWorkitemInBaselineAsync rather than SearchWorkitemInRevisionAsync because
    /// baseline correctly returns all items including cross-project references (Revision variant
    /// omits 1 item per timing study on rev 643133).
    ///
    /// Step 4 does not call GetRevisionIdsAsync or GetWorkItemByUriAsync per item. Timing analysis
    /// showed those calls account for ~99% of execution time (450s for 1395 items) while
    /// IsHistorical=false occurred in only 0.14% of cases — not worth the cost.
    /// </remarks>
    /// <param name="moduleFolder">The module folder path (e.g., "L4_fcs")</param>
    /// <param name="documentId">The document ID (e.g., "FCS Memory Loader IDD")</param>
    /// <param name="revision">The revision number</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items with revision information</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItemWithRevisionInfo[]>> GetWorkItemsByModuleRevisionAsync(
        string moduleFolder,
        string documentId,
        string revision,
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

        if (string.IsNullOrWhiteSpace(revision))
        {
            return Result.Fail("Revision cannot be null or empty");
        }

        var fieldList = fields ?? DefaultWorkItemFields;

        fieldList.Remove("uri");

        // Step 1: Get module by location to obtain its real URI
        var location = $"{moduleFolder}/{documentId}";
        var moduleResult = await GetModuleByLocationAsync(location);

        if (moduleResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                $"Failed to get module at location '{location}': {moduleResult.Errors.First().Message}");
        }

        var module = moduleResult.Value;
        if (string.IsNullOrEmpty(module?.uri))
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                $"Module at location '{location}' has no URI");
        }

        // Step 2: Append revision to URI, get work item URIs, extract IDs and per-item revisions
        var moduleUriWithRevision = $"{module.uri}%{revision}";
        var urisResult = await GetModuleWorkItemUrisAsync(moduleUriWithRevision, null, true);

        if (urisResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                $"Failed to get work item URIs: {urisResult.Errors.First().Message}");
        }

        var workItemUris = urisResult.Value;
        if (workItemUris.Length == 0)
        {
            return Result.Ok(Array.Empty<WorkItemWithRevisionInfo>());
        }

        var wiRevisionMap = new Dictionary<string, (string Revision, string Uri)>();
        foreach (var uri in workItemUris)
        {
            var wiId = PolarionUriParser.ExtractIdFromUri(uri);
            var wiRev = PolarionUriParser.ExtractRevisionFromUri(uri);

            if (!string.IsNullOrEmpty(wiId))
            {
                wiRevisionMap[wiId] = (wiRev, uri);
            }
        }

        if (wiRevisionMap.Count == 0)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                "No valid work item IDs could be extracted from URIs");
        }

        // Step 3: Bulk fetch work item data at the specified baseline revision
        var ids = string.Join(" ", wiRevisionMap.Keys);
        var query = $"id:({ids})";
        var workItemsResult = await SearchWorkitemInBaselineAsync(revision, query, "id", fieldList, includeAllProjects: true);

        if (workItemsResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                $"Failed to bulk fetch work items at revision {revision}: {workItemsResult.Errors.First().Message}");
        }

        // Step 4: Wrap results — all items are historical by definition (revision query)
        var finalWorkItems = new List<WorkItemWithRevisionInfo>();

        foreach (var workItem in workItemsResult.Value)
        {
            if (workItem?.id is null || !wiRevisionMap.TryGetValue(workItem.id, out var revisionInfo))
            {
                continue;
            }

            var (targetRevision, wiUri) = revisionInfo;

            finalWorkItems.Add(new WorkItemWithRevisionInfo
            {
                WorkItem = workItem,
                Revision = targetRevision,
                HeadRevision = string.Empty,
                IsHistorical = true,
                SourceUri = wiUri
            });
        }

        return Result.Ok(finalWorkItems.ToArray());
    }
}
