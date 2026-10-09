namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    internal const string GetWorkItemsByModuleObsoleteMessage =
        "Returns only work items owned by the document; referenced and pinned items are omitted. " +
        "Use GetModuleWorkItemsAsync or QueryWorkItemsInModuleAsync.";

    /// <summary>
    /// Fetches the work items owned by a module (document), found by title, using a Lucene query.
    /// </summary>
    /// <param name="moduleTitle">The title of the module to fetch data from</param>
    /// <param name="filter">The filter criteria for work items</param>
    /// <param name="moduleRevision">Optional baseline revision. If null, fetches from HEAD</param>
    /// <returns>A Result containing the owned work items that have an outline number. A query that matches
    /// nothing is a successful, empty result when the module exists, and a failure when no module title contains that
    /// title (or the module does not exist at <paramref name="moduleRevision"/>)</returns>
    /// <remarks>
    /// Deprecated: the <c>document.title</c> Lucene query only matches items owned by the document, so
    /// referenced and pinned items are omitted, and results follow <see cref="PolarionFilter.Order"/>
    /// rather than document order. Use <see cref="GetModuleWorkItemsAsync"/> or
    /// <see cref="QueryWorkItemsInModuleAsync"/> instead.
    /// </remarks>
    [Obsolete(GetWorkItemsByModuleObsoleteMessage)]
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> GetWorkItemsByModuleAsync(string moduleTitle, PolarionFilter filter, string? moduleRevision = null)
    {
        string text = "document.title:\"" + moduleTitle + "\"";
        string query = string.IsNullOrWhiteSpace(filter.WorkItemFilter) ? text : filter.WorkItemFilter + " AND " + text;
        WorkItem[] value;
        if (moduleRevision == null)
        {
            Result<WorkItem[]> result = await SearchWorkitemAsync(query, filter.Order, filter.Fields);
            if (result.IsFailed)
            {
                return Result.Fail<WorkItem[]>("Failed to fetch data: " + result.Errors.First());
            }

            value = result.Value;
        }
        else
        {
            Result<WorkItem[]> result2 = await SearchWorkitemInBaselineAsync(moduleRevision, query, filter.Order, filter.Fields);
            if (result2.IsFailed)
            {
                return Result.Fail<WorkItem[]>("Failed to fetch data: " + result2.Errors.First());
            }

            value = result2.Value;
        }

        List<WorkItem> list = new List<WorkItem>();
        WorkItem[] array = value;
        foreach (WorkItem workItem in array)
        {
            if (workItem.outlineNumber != null) // if a workitem doesn't have an outline number, then is not part of a module.
            {
                list.Add(workItem);
            }
        }

        value = [.. list];

        // A zero-row query does not say whether the module is empty or missing, so ask the server.
        if (value.Length == 0)
        {
            var exists = await EnsureModuleExistsByTitleAsync(moduleTitle, moduleRevision);
            if (exists.IsFailed)
            {
                return Result.Fail<WorkItem[]>(exists.Errors);
            }
        }

        return value;
    }

    /// <summary>
    /// Tells an empty document from a missing one after a title-based module read returned no rows.
    /// </summary>
    /// <remarks>
    /// The work item query matches <c>document.title</c> as a case-insensitive phrase, so a module counts as
    /// found when its title contains <paramref name="moduleTitle"/> (the same containment
    /// <see cref="GetModulesThinAsync"/> applies); an exact match is not required because a document's own
    /// title can carry a suffix the work items' <c>document.title</c> does not. When a revision is given, the
    /// module must also resolve at <c>{moduleUri}%{revision}</c>, the same way the URI-based module reads
    /// check it; any matching module that resolves is enough. Only called on the zero-row path, so reads that
    /// return rows cost no extra call.
    /// </remarks>
    /// <param name="moduleTitle">The module title that was queried</param>
    /// <param name="moduleRevision">The baseline revision that was queried, or null for HEAD</param>
    /// <returns>Ok when a matching module exists (at the revision, if given); otherwise a failure</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    private async Task<Result> EnsureModuleExistsByTitleAsync(string moduleTitle, string? moduleRevision)
    {
        var modulesResult = await GetModulesThinAsync(titleContains: moduleTitle);
        if (modulesResult.IsFailed)
        {
            return Result.Fail($"Failed to look up document '{moduleTitle}': {modulesResult.Errors.First().Message}");
        }

        // Exact title first, so the common case needs a single revision lookup.
        var candidates = modulesResult.Value
            .OrderByDescending(m => string.Equals(m.Title, moduleTitle, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (candidates.Length == 0)
        {
            return Result.Fail($"Document not found: '{moduleTitle}'");
        }

        if (string.IsNullOrWhiteSpace(moduleRevision))
        {
            return Result.Ok();
        }

        var revision = moduleRevision.Trim();
        var notFoundMessage = $"Document not found at revision {revision}: '{moduleTitle}'";
        Result? lastFailure = null;
        foreach (var module in candidates)
        {
            var resolvable = await EnsureModuleResolvableAsync($"{module.Uri}%{revision}", notFoundMessage);
            if (resolvable.IsSuccess)
            {
                return Result.Ok();
            }

            lastFailure = resolvable;
        }

        return lastFailure!;
    }
}
