using System.IO.Pipelines;
using System.Threading.Tasks;



namespace Polarion;

public interface IPolarionClient
{
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId);
    Task<Result<WorkItem[]>> SearchWorkitem(string query, string order, List<string> field_list);
    Task<Result<WorkItem[]>> SearchWorkitemInBaseline(string baselineRevision, string query, string order, List<string> field_list);
}

