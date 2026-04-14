using HtmlAgilityPack;

namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Query for available workitems. This will only query for the items.
    /// If you also want the Workitems to be retrieved, used searchWorkitemFullItem.
    ///
    /// For retrieving custom field using field_list, use the following syntax:
    /// fieldList=['customFields.SomeField']
    /// </summary>
    /// <param name="query">The query to use while searching</param>
    /// <param name="order">Order by</param>
    /// <param name="fieldList">list of fields to retrieve for each search result</param>
    /// <param name="includeAllProjects">When true, omits the project.id filter so results span all projects. Default is false.</param>
    /// <returns>Search results but only with the given fields set</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order = "Created", List<string>? fieldList = null, bool includeAllProjects = false)
    {
        bool isReuseDocument = true;

        var result = isReuseDocument ?
            await SearchWorkitemAsyncComplex(query, order, fieldList, includeAllProjects) :
            await SearchWorkitemAsyncSimple(query, order, fieldList, includeAllProjects);

        return result;
    }

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemAsyncSimple(string query, string order = "Created", List<string>? fieldList = null, bool includeAllProjects = false)
    {
        try
        {
            if (fieldList is null)
            {
                fieldList = ["id"];
            }

            if (!includeAllProjects)
            {
                query += $" AND project.id:{_config.ProjectId}";
            }

            var result = await _trackerClient.queryWorkItemsAsync(new(query, order, [.. fieldList]));
            return result.queryWorkItemsReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemAsyncComplex(string query, string order = "Created", List<string>? fieldList = null, bool includeAllProjects = false)
    {
        try
        {
            if (fieldList is null)
            {
                fieldList = ["id"];
            }

            if (!includeAllProjects)
            {
                query += $" AND project.id:{_config.ProjectId}";
            }

            var result = await _trackerClient.queryWorkItemsAsync(new(query, order, [.. fieldList]));
            return result.queryWorkItemsReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }
}
