namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Exports Polarion work items grouped by heading level to Markdown format asynchronously.
    /// </summary>
    /// <param name="headingLevel">The heading level to group by.</param>
    /// <param name="workItemPrefix">The prefix used for work item IDs.</param>
    /// <param name="moduleTitle">The title of the module to export.</param>
    /// <param name="filter">The filter criteria for work items.</param>
    /// <param name="workItemTypeToShortNameMap">A dictionary mapping work item type IDs to short names.</param>
    /// <param name="revision">Optional revision identifier. If null, exports the latest revision.</param>
    /// <returns>A Result containing a SortedDictionary of heading-grouped Markdown content if successful.</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<SortedDictionary<string, StringBuilder>>> ExportModuleToMarkdownGroupedByHeadingAsync(int headingLevel, string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, string? revision = null)
    {
        if (string.IsNullOrWhiteSpace(moduleTitle))
        {
            return Result.Fail<SortedDictionary<string, StringBuilder>>("Title cannot be null or empty");
        }

        var result = await GetHierarchicalWorkItemsByModuleAsync(workItemPrefix, moduleTitle, filter, revision);
        if (result.IsFailed)
        {
            return Result.Fail<SortedDictionary<string, StringBuilder>>("Failed to fetch data: " + result.Errors.First());
        }

        var moduleWorkItems = result.Value;
        
        var workItemsGroupedByHeading = new SortedDictionary<string, List<WorkItem>>();
        var activeContent = new List<WorkItem>();
        

        foreach ((var outlineNumber, var workItemsByOutlineNumber) in moduleWorkItems)
        {
            if (workItemsByOutlineNumber.Count == 0)
            {
                continue;
            }

            foreach (var workItem in workItemsByOutlineNumber.Values)
            {
                if (workItem.type.id == "heading")
                {
                    if (workItem.outlineNumber.Count(c => c == '.') + 1 == headingLevel)
                    {
                        var headingTitle = workItem.title.Replace(' ', '_').Replace('/', '_').Replace('\\', '_');
                        var key = $"{workItem.outlineNumber}_{headingTitle}";
                        workItemsGroupedByHeading.Add(key, [workItem]);
                        activeContent = workItemsGroupedByHeading[key];
                    }
                    else
                    {
                        activeContent.Add(workItem);
                    }
                }
                else
                {
                    activeContent.Add(workItem);
                }
            }
        }

        // loop the contentGroupedByHeading and build the markdown strings
        var contentGroupedByHeading = new SortedDictionary<string, StringBuilder>();
        foreach ((var key, var value) in workItemsGroupedByHeading)
        {
            foreach (var workItem in value)
            {
                if (contentGroupedByHeading.TryGetValue(key, out var markdownBuilder))
                {
                    ConvertToMarkdown(workItem, workItemTypeToShortNameMap, markdownBuilder);
                }
                else
                {
                    markdownBuilder = new StringBuilder();
                    ConvertToMarkdown(workItem, workItemTypeToShortNameMap, markdownBuilder);
                    contentGroupedByHeading.Add(key, markdownBuilder);
                }
            }
        }


        return contentGroupedByHeading;
    }
}