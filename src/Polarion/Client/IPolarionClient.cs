namespace Polarion;

public interface IPolarionClient
{
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId, string? revision = null);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order, List<string> field_list, bool includeAllProjects = false);

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(string baselineRevision, string query, string order, List<string> field_list, bool includeAllProjects = false);

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
    [Obsolete(PolarionClient.GetWorkItemsByModuleObsoleteMessage)]
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
    /// <returns>Array of work item URIs; empty for an empty document. A failure for a URI with no document
    /// (Polarion raises an unresolvable-object error; as a fallback, "Document not found" when no URIs come
    /// back and the module is unresolvable)</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<string[]>> GetModuleWorkItemUrisAsync(string moduleUri, string? parentWorkItemUri = null, bool deep = true);

    /// <summary>
    /// Gets the work items of a document (module) in document order, including referenced items.
    /// </summary>
    /// <param name="moduleUri">The module URI, optionally with a revision suffix (<c>moduleUri%revision</c>)</param>
    /// <param name="parentWorkItemUri">Optional parent work item URI; when set, only its children are returned</param>
    /// <param name="deep">When true (default), returns the whole tree; when false, only direct children</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>One row per document entry, in document order; pinned revisions and unresolvable rows are exposed</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<ModuleWorkItem[]>> GetModuleWorkItemsAsync(string moduleUri, string? parentWorkItemUri = null, bool deep = true, List<string>? fields = null);

    /// <summary>
    /// Queries baselines (project and document baselines) with a Lucene query, scoped to the
    /// configured project by default.
    /// </summary>
    /// <param name="query">Lucene query, passed unchanged</param>
    /// <param name="sort">Sort field (default: baseRevision)</param>
    /// <param name="includeAllProjects">When false (default), only baselines whose baseObjectURI belongs to
    /// the configured project are returned; when true, baselines of every project are returned</param>
    /// <returns>The matching baselines; empty when nothing matches</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Baseline[]>> QueryBaselinesAsync(string query, string sort = "baseRevision", bool includeAllProjects = false);

    /// <summary>
    /// Queries the URIs of documents (modules) as they existed at a baseline revision.
    /// </summary>
    /// <param name="baselineRevision">The baseline's base revision</param>
    /// <param name="query">Lucene query over modules, passed unchanged</param>
    /// <param name="sort">Sort field (default: uri)</param>
    /// <param name="limit">Maximum number of results (-1 = all)</param>
    /// <returns>The module URIs; empty when nothing matches</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<string[]>> QueryModuleUrisInBaselineAsync(string baselineRevision, string query, string sort = "uri", int limit = -1);

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
    /// Queries work items from a module at a specific historical revision, in document order.
    /// </summary>
    /// <remarks>
    /// Built on <see cref="GetModuleWorkItemsAsync"/> with <c>{moduleUri}%{revision}</c>. Pinned references
    /// are returned at their pinned revision (reported in <see cref="WorkItemWithRevisionInfo.Revision"/>),
    /// deleted-but-pinned items are included, and unresolvable rows are dropped. A location with no
    /// document at that revision fails (Polarion raises an unresolvable-object error; as a fallback,
    /// "Document not found" when no rows come back and the module is unresolvable at that revision).
    /// </remarks>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="revision">The revision number (digits only; surrounding whitespace is trimmed)</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items with revision information</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItemWithRevisionInfo[]>> GetWorkItemsByModuleRevisionAsync(string moduleFolder, string documentId, string revision, List<string>? fields = null);

    /// <summary>
    /// Gets the work items of a document (module) at HEAD, in document order.
    /// </summary>
    /// <remarks>
    /// Built on <see cref="GetModuleWorkItemsAsync"/>. Pinned references are returned at their pinned
    /// revision, deleted-but-pinned items are included, and unresolvable rows are dropped.
    /// </remarks>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="itemTypes">Optional list of work item type IDs to keep</param>
    /// <param name="sort">Ignored; results are always in document order</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items in the module</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<WorkItem[]>> QueryWorkItemsInModuleAsync(string moduleFolder, string documentId, List<string>? itemTypes = null, string sort = "outlineNumber", List<string>? fields = null);

    /// <summary>
    /// Gets the users explicitly assigned to a project (its member list).
    /// </summary>
    /// <param name="projectId">The Polarion project ID</param>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Polarion.Generated.Project.User[]>> GetProjectUsersAsync(string projectId);

    /// <summary>
    /// Gets every user known to this Polarion instance. Takes no filter — callers resolving a
    /// single user by email or id must filter the result themselves and should cache it.
    /// </summary>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    Task<Result<Polarion.Generated.Project.User[]>> GetUsersAsync();

    TrackerWebService TrackerService { get; }

    Polarion.Generated.Project.ProjectWebService ProjectService { get; }
}
