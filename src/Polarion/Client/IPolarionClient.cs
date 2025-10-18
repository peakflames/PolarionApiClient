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
    Task<Result<ModuleThin[]>> GetModulesInSpaceThinAsync(string spaceName);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<List<string>>> GetSpacesAsync(string? excludeSpaceNameContains = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<ModuleThin[]>> GetModulesThinAsync(string? excludeSpaceNameContains = null, string? titleContains = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Module>> GetModuleByLocationAsync(string location);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Module>> GetModuleByUriAsync(string uri);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> GetWorkItemsByModuleAsync(string moduleTitle, PolarionFilter filter, string? moduleRevision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<SortedDictionary<string, SortedDictionary<string, WorkItem>>>> GetHierarchicalWorkItemsByModuleAsync(
        string workItemPrefix, string moduleTitle, PolarionFilter filter, string? moduleRevision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<StringBuilder>> ExportModuleToMarkdownAsync(
        string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, bool includeWorkItemIdentifiers = true, string? revision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<SortedDictionary<string, StringBuilder>>> ExportModuleToMarkdownGroupedByHeadingAsync(
        int headingLevel, string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, bool includeWorkItemIdentifiers = true, string? revision = null);

    [RequiresUnreferencedCode("Uses ReverseMarkdown which requires reflection")]
    string ConvertWorkItemToMarkdown(string workItemId, WorkItem? workItem, string? errorMsgPrefix = null, bool includeMetadata = false);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<string[]>> GetRevisionIdsAsync(string uri);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<string[]>> GetRevisionsIdsByWorkItemIdAsync(string workItemId);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> GetWorkItemRevisionsByIdAsync(string workItemId, int maxRevisions = -1);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Module[]>> GetModuleRevisionsByLocationAsync(string location, int maxRevisions = -1);

    TrackerWebService TrackerService { get; }
}
