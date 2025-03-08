using System.IO.Pipelines;
using System.Threading.Tasks;



namespace PolarionApiClient.Core;

public interface IPolarionClient
{
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId);
    Task<Result<WorkItem[]>> QueryWorkItemsAsync(string query, string sort, string[] fields);
}

