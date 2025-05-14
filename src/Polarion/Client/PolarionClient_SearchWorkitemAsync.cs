using System.Net;
using System.Reflection.Metadata;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml.Linq;


namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Query for available workitems. This will only query for the items.
    /// If you also want the Workitems to be retrieved, used searchWorkitemFullItem.
    ///
    /// For retrieving custom field using field_list, use the following syntax:
    /// field_list=['customFields.SomeField']
    /// </summary>
    /// <param name="query">The query to use while searching</param>
    /// <param name="order">Order by</param>
    /// <param name="field_list">list of fields to retrieve for each search result</param>
    /// <returns>Search results but only with the given fields set</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order = "Created", List<string>? field_list = null)
    {
        try
        {
            if (field_list is null)
            {
                field_list = ["id"];
            }

            query += $" AND project.id:{_config.ProjectId}";

            var result = await _trackerClient.queryWorkItemsAsync(new(query, order, field_list.ToArray()));
            return result.queryWorkItemsReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }
}
