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
    Task<Result<WorkItem[]>> GetWorkItemsByModuleAsync(string moduleTitle, PolarionFilter filter, string? moduleRevision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<SortedDictionary<string, SortedDictionary<string, WorkItem>>>> GetHierarchicalWorkItemsByModuleAsync(
        string workItemPrefix, string moduleTitle, PolarionFilter filter, string? moduleRevision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<StringBuilder>> ExportModuleToMarkdownAsync(
        string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, string? revision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public Task<Result<SortedDictionary<string, StringBuilder>>> ExportModuleToMarkdownGroupedByHeadingAsync(
        int headingLevel, string workItemPrefix, string moduleTitle, PolarionFilter filter, Dictionary<string, string> workItemTypeToShortNameMap, string? revision = null);

    TrackerWebService TrackerService { get; }
}
