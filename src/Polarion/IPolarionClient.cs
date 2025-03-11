namespace Polarion;

public interface IPolarionClient
{
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId);
    Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order, List<string> field_list);
    Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(string baselineRevision, string query, string order, List<string> field_list);
    Task<Result<Module[]>> GetDocumentsInSpaceAsync(string spaceName);
}

