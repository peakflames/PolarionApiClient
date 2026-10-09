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
    /// <returns>A Result containing the owned work items that have an outline number</returns>
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
        return value;
    }
}
