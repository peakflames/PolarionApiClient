
namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Query for available workitems in a baseline. This will only query for the items.
    /// If you also want the Workitems to be retrieved, used searchWorkitemFullItemInBaseline.
    ///
    /// For retrieving custom field using field_list, use the following syntax:
    /// field_list=['customFields.FieldName']
    /// </summary>
    /// <param name="baselineRevision">The revision number of the baseline to search in</param>
    /// <param name="query">The query to use while searching</param>
    /// <param name="order">Order by</param>
    /// <param name="field_list">list of fields to retrieve for each search result</param>
    /// <returns>Search results but only with the given fields set</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(string baselineRevision, string query, string order = "Created", List<string>? field_list = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baselineRevision))
            {
                return Result.Fail<WorkItem[]>("Baseline revision cannot be null or empty");
            }

            if (field_list is null)
            {
                field_list = ["id"];
            }

            query += $" AND project.id:{_config.ProjectId}";

            var result = await _trackerClient.queryWorkItemsInRevisionAsync(new(query, order, baselineRevision,field_list.ToArray()));
            return result.queryWorkItemsInRevisionReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsInRevisionReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }
}
