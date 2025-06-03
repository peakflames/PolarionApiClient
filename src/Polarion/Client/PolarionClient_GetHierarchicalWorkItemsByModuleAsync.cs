namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Fetches work items from Polarion and organizes them into a hierarchical structure.
    /// </summary>
    /// <param name="workItemPrefix">The prefix used for work item IDs.</param>
    /// <param name="moduleTitle">The title of the module to fetch data from.</param>
    /// <param name="filter">The filter criteria for work items.</param>
    /// <param name="moduleRevision">Optional revision identifier. If null, fetches from the latest revision.</param>
    /// <returns>A Result containing a hierarchical dictionary of WorkItems if successful.</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<SortedDictionary<string, SortedDictionary<string, WorkItem>>>> GetHierarchicalWorkItemsByModuleAsync(
        string workItemPrefix, string moduleTitle, PolarionFilter filter, string? moduleRevision = null) 
    {
        var result = await GetWorkItemsByModuleAsync(moduleTitle, filter, moduleRevision);
        if (result.IsFailed)
        {
            return Result.Fail<SortedDictionary<string, SortedDictionary<string, WorkItem>>>("Failed to fetch data: " + result.Errors.First());
        }

        var workItems = result.Value;

        var outlineComparer = new OutlineNumberComparer();
        var objectHierarchy = new SortedDictionary<string, SortedDictionary<string, WorkItem>>(outlineComparer);
        var targetItemPrefix = $"{workItemPrefix}-";

        foreach (var workItem in workItems)
        {
            var scrubbedOutlineNumber = workItem.outlineNumber.Replace(targetItemPrefix, "");
            // if the outlineNumber contains a hyphen, then it is a child to a header
            if (scrubbedOutlineNumber.Contains('-'))
            {
                var parentHeading = scrubbedOutlineNumber.Split('-')[0];
                if (!objectHierarchy.ContainsKey(parentHeading))
                {
                    objectHierarchy[parentHeading] = new SortedDictionary<string, WorkItem>(outlineComparer);
                }
                objectHierarchy[parentHeading][scrubbedOutlineNumber] = workItem;
            }
            else
            {
                if (!objectHierarchy.ContainsKey(scrubbedOutlineNumber))
                {
                    objectHierarchy[scrubbedOutlineNumber] = new SortedDictionary<string, WorkItem>(outlineComparer);
                }
                objectHierarchy[scrubbedOutlineNumber][scrubbedOutlineNumber] = workItem;
            }
        }


        return objectHierarchy;
    }
}
