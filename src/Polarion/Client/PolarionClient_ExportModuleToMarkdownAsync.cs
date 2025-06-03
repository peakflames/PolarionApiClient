namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Exports Polarion work items to Markdown format asynchronously.
    /// </summary>
    /// <param name="workItemPrefix">The prefix used for work item IDs.</param>
    /// <param name="moduleTitle">The title of the module to export.</param>
    /// <param name="filter">The filter criteria for work items.</param>
    /// <param name="workItemTypeToShortNameMap">A dictionary mapping work item type IDs to short names.</param>
    /// <param name="revision">Optional revision identifier. If null, exports the latest revision.</param>
    /// <returns>A Result containing a StringBuilder with the Markdown content if successful.</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<StringBuilder>> ExportModuleToMarkdownAsync(string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, string? revision = null)
    {
        if (string.IsNullOrWhiteSpace(moduleTitle))
        {
            return Result.Fail<StringBuilder>("Title cannot be null or empty");
        }

        var result = await GetHierarchicalWorkItemsByModuleAsync(workItemPrefix, moduleTitle, filter, revision);
        if (result.IsFailed)
        {
            return Result.Fail<StringBuilder>("Failed to fetch data: " + result.Errors.First());
        }

        var moduleWorkItems = result.Value;
        var stringBuilder = new StringBuilder();

        foreach (var entry in moduleWorkItems)
        {
            if (!entry.Value.Any())
            {
                continue;
            }

            foreach (var workItemEntry in entry.Value)
            {
                ConvertToMarkdown(workItemEntry.Value, workItemTypeToShortNameMap, stringBuilder);
            }
        }


        return stringBuilder;
    }

    


    

    

}