namespace Polarion;

public interface IPolarionClient
{
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId, string? revision = null);

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
    Task<Result<Dictionary<string, WorkItem>>> GetWorkItemRevisionsByIdAsync(string workItemId, int maxRevisions = -1);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Module[]>> GetModuleRevisionsByLocationAsync(string location, int maxRevisions = -1);

    /// <summary>
    /// Gets URIs of all work items in a module at the specified revision.
    /// </summary>
    /// <param name="moduleUri">The module URI (may include revision specifier)</param>
    /// <param name="parentWorkItemUri">Optional parent work item URI to filter children</param>
    /// <param name="deep">Whether to include external/linked items</param>
    /// <returns>Array of work item URIs</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<string[]>> GetModuleWorkItemUrisAsync(string moduleUri, string? parentWorkItemUri = null, bool deep = true);

    /// <summary>
    /// Gets a work item by its URI (the URI may include a revision specifier).
    /// </summary>
    /// <param name="uri">The Polarion work item URI</param>
    /// <returns>The work item at the URI's embedded revision</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem>> GetWorkItemByUriAsync(string uri);

    /// <summary>
    /// Gets a work item at a specific revision using its URI.
    /// </summary>
    /// <param name="uri">The Polarion work item URI</param>
    /// <param name="revision">The revision to retrieve</param>
    /// <returns>The work item at the specified revision</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem>> GetWorkItemByUriInRevisionAsync(string uri, string revision);

    /// <summary>
    /// Queries work items from a branched document using the 4-step revision-aware algorithm.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// 1. Get URIs for the specific revision document
    /// 2. Extract work item IDs and revisions from URIs
    /// 3. Bulk fetch HEAD versions using Lucene query
    /// 4. Fetch historical versions where revisions differ from HEAD
    /// </remarks>
    /// <param name="moduleFolder">The module folder path (e.g., "FCC_L4_Air8_1")</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="revision">The revision number</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items with revision information</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItemWithRevisionInfo[]>> GetWorkItemsByModuleRevisionAsync(string moduleFolder, string documentId, string revision, List<string>? fields = null);

    /// <summary>
    /// Queries work items using SQL against POLARION.REL_MODULE_WORKITEM relationship.
    /// </summary>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="itemTypes">Optional list of work item types to filter</param>
    /// <param name="sort">Sort field (default: outlineNumber)</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items in the module</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> QueryWorkItemsInModuleAsync(string moduleFolder, string documentId, List<string>? itemTypes = null, string sort = "outlineNumber", List<string>? fields = null);

    TrackerWebService TrackerService { get; }
}
