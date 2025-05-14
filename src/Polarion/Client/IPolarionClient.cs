namespace Polarion;

public interface IPolarionClient
{
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order, List<string> field_list);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(string baselineRevision, string query, string order, List<string> field_list);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Module[]>> GetDocumentsInSpaceAsync(string spaceName);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<List<string>>> GetDocumentSpacesAsync();
}
