namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Default fields to retrieve when querying work items.
    /// </summary>
    private static readonly List<string> DefaultWorkItemFields =
    [
        "id", "type", "title", "description", "status", "outlineNumber", "uri"
    ];

    /// <summary>
    /// Queries work items from a branched document using the 4-step revision-aware algorithm.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// 1. Get module by location to obtain its real URI
    /// 2. Append revision to URI and get work item URIs
    /// 3. Extract work item IDs and revisions from URIs
    /// 4. Bulk fetch HEAD versions using Lucene query
    /// 5. Fetch historical versions where revisions differ from HEAD
    /// 
    /// Based on verified Python implementation in ple_systest_utils/polarion.py
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
            return Result.Fail<WorkItemWithRevisionInfo[]>($"Failed to get module at location '{location}': {moduleResult.Errors.First().Message}");
        }

        var module = moduleResult.Value;
        if (string.IsNullOrEmpty(module?.uri))
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>($"Module at location '{location}' has no URI");
        }

        // Step 2: Append revision to the module's URI and get work item URIs
        // The module URI format: subterra:data-service:objects:/default/{Project}${Module}{moduleFolder}{Space}#{ModuleWorkItemId}
        // We append %{revision} to query at that specific revision
        var moduleUriWithRevision = $"{module.uri}%{revision}";
        var urisResult = await GetModuleWorkItemUrisAsync(moduleUriWithRevision, null, true);

        if (urisResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>($"Failed to get work item URIs: {urisResult.Errors.First().Message}");
        }

        var workItemUris = urisResult.Value;
        if (workItemUris.Length == 0)
        {
            return Result.Ok(Array.Empty<WorkItemWithRevisionInfo>());
        }

        // Step 2: Extract work item IDs and revisions from URIs
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
            return Result.Fail<WorkItemWithRevisionInfo[]>("No valid work item IDs could be extracted from URIs");
        }

        // Step 3: Bulk fetch HEAD versions using Lucene query
        var ids = string.Join(" ", wiRevisionMap.Keys);
        var query = $"id:({ids})"; // Note: SearchWorkitemAsync adds project.id filter automatically
        var headWorkItemsResult = await SearchWorkitemAsync(query, "id", fieldList);

        if (headWorkItemsResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>($"Failed to bulk fetch HEAD work items: {headWorkItemsResult.Errors.First().Message}");
        }

        // Step 4: Fetch historical versions where revisions differ from HEAD
        var finalWorkItems = new List<WorkItemWithRevisionInfo>();

        foreach (var headWi in headWorkItemsResult.Value)
        {
            if (headWi?.id is null || !wiRevisionMap.TryGetValue(headWi.id, out var revisionInfo))
            {
                continue;
            }

            var (targetRevision, wiUri) = revisionInfo;

            // Get HEAD revision for comparison
            var revisionsResult = await GetRevisionIdsAsync(wiUri);
            if (revisionsResult.IsFailed || revisionsResult.Value.Length == 0)
            {
                // If we can't get revisions, use the HEAD work item
                finalWorkItems.Add(new WorkItemWithRevisionInfo
                {
                    WorkItem = headWi,
                    Revision = targetRevision,
                    HeadRevision = targetRevision,
                    IsHistorical = false,
                    SourceUri = wiUri
                });
                continue;
            }

            var headRevision = revisionsResult.Value.Last();

            if (headRevision != targetRevision)
            {
                // Revisions differ - fetch historical version
                var historicalWiResult = await GetWorkItemByUriAsync(wiUri);

                if (historicalWiResult.IsSuccess)
                {
                    finalWorkItems.Add(new WorkItemWithRevisionInfo
                    {
                        WorkItem = historicalWiResult.Value,
                        Revision = targetRevision,
                        HeadRevision = headRevision,
                        IsHistorical = true,
                        SourceUri = wiUri
                    });
                }
                else
                {
                    // Fall back to HEAD if historical fetch fails
                    finalWorkItems.Add(new WorkItemWithRevisionInfo
                    {
                        WorkItem = headWi,
                        Revision = targetRevision,
                        HeadRevision = headRevision,
                        IsHistorical = false,
                        SourceUri = wiUri
                    });
                }
            }
            else
            {
                // Same revision - use fast-fetched HEAD
                finalWorkItems.Add(new WorkItemWithRevisionInfo
                {
                    WorkItem = headWi,
                    Revision = targetRevision,
                    HeadRevision = headRevision,
                    IsHistorical = false,
                    SourceUri = wiUri
                });
            }
        }

        return Result.Ok(finalWorkItems.ToArray());
    }
}
