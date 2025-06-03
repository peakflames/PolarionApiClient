namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    //
    // Summary:
    //     Fetches work items from Polarion based on specified criteria.
    //
    // Parameters:
    //   polarionClient:
    //     The client used to communicate with Polarion.
    //
    //   moduleTitle:
    //     The title of the module to fetch data from.
    //
    //   filter:
    //     The filter criteria for work items.
    //
    //   moduleRevision:
    //     Optional revision identifier. If null, fetches from the latest revision.
    //
    // Returns:
    //     A Result containing an array of WorkItems if successful.
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
