# Polarion API Client - API Documentation

## Overview

The Polarion API Client is a .NET library for interacting with Polarion ALM (Application Lifecycle Management) systems. It provides asynchronous methods for querying, retrieving, and exporting work items and modules.

## Table of Contents

- [Polarion API Client - API Documentation](#polarion-api-client---api-documentation)
  - [Overview](#overview)
  - [Table of Contents](#table-of-contents)
  - [Client Initialization](#client-initialization)
    - [CreateAsync](#createasync)
  - [Work Item Operations](#work-item-operations)
    - [GetWorkItemByIdAsync](#getworkitembyidasync)
    - [SearchWorkitemAsync](#searchworkitemasync)
    - [SearchWorkitemInBaselineAsync](#searchworkiteminbaselineasync)
    - [GetWorkItemsByModuleAsync](#getworkitemsbymoduleasync)
    - [GetHierarchicalWorkItemsByModuleAsync](#gethierarchicalworkitemsbymoduleasync)
  - [Module Operations](#module-operations)
    - [GetModulesInSpaceThinAsync](#getmodulesinspacethinasync)
    - [GetModulesThinAsync](#getmodulesthinasync)
    - [GetModuleByLocationAsync](#getmodulebylocationasync)
    - [GetModuleByUriAsync](#getmodulebyuriasync)
    - [GetModuleWorkItemUrisAsync](#getmoduleworkitemurisasync)
    - [GetModuleWorkItemsAsync](#getmoduleworkitemsasync)
    - [QueryWorkItemsInModuleAsync](#queryworkitemsinmoduleasync)
    - [GetWorkItemsByModuleRevisionAsync](#getworkitemsbymodulerevisionasync)
  - [Baseline Operations](#baseline-operations)
    - [QueryBaselinesAsync](#querybaselinesasync)
    - [QueryModuleUrisInBaselineAsync](#querymoduleurisinbaselineasync)
  - [Space Operations](#space-operations)
    - [GetSpacesAsync](#getspacesasync)
  - [User Operations](#user-operations)
    - [GetProjectUsersAsync](#getprojectusersasync)
    - [GetUsersAsync](#getusersasync)
  - [Markdown Export Operations](#markdown-export-operations)
    - [ExportModuleToMarkdownAsync](#exportmoduletomarkdownasync)
    - [ExportModuleToMarkdownGroupedByHeadingAsync](#exportmoduletomarkdowngroupedbyheadingasync)
    - [ConvertWorkItemToMarkdown](#convertworkitemtomarkdown)
  - [Revision Operations](#revision-operations)
    - [GetRevisionIdsAsync](#getrevisionidsasync)
    - [GetRevisionsIdsByWorkItemIdAsync](#getrevisionsidsbyworkitemidasync)
    - [GetWorkItemRevisionsByIdAsync](#getworkitemrevisionsbyidasync)
    - [GetModuleRevisionsByLocationAsync](#getmodulerevisionsbylocationasync)
  - [Configuration](#configuration)
    - [PolarionClientConfiguration](#polarionclientconfiguration)
  - [Properties](#properties)
    - [TrackerService](#trackerservice)
    - [ProjectService](#projectservice)
  - [Notes](#notes)

---

## Client Initialization

### CreateAsync

```csharp
public static async Task<Result<PolarionClient>> CreateAsync(PolarionClientConfiguration config)
```

Creates and initializes a new Polarion client instance with the provided configuration. Establishes a connection to the Polarion server, authenticates the user, and sets up the necessary WCF service clients.

**Parameters:**
- `config` - The configuration object containing server URL, credentials, and project ID

**Returns:** A `Result<PolarionClient>` containing the initialized client or error details

**Remarks:** Uses WCF services which require reflection. Configures HTTP binding with appropriate timeouts and message size limits.

---

## Work Item Operations

### GetWorkItemByIdAsync

```csharp
public async Task<Result<WorkItem>> GetWorkItemByIdAsync(
    string workItemId, 
    string? revision = null)
```

Retrieves a single work item by its ID, optionally at a specific revision.

**Parameters:**
- `workItemId` - The unique identifier of the work item
- `revision` - Optional revision ID. If null, returns the latest version (default: null)

**Returns:** A `Result<WorkItem>` containing the work item at the specified revision or latest if revision is null, or error details

**Throws:** `PolarionClientException` if the operation fails

**Remarks:** When no revision is specified, uses a single optimized API call. When a revision is specified, makes two API calls to first obtain the work item URI, then retrieve the specific revision.

---

### SearchWorkitemAsync

```csharp
public async Task<Result<WorkItem[]>> SearchWorkitemAsync(
    string query, 
    string order = "Created", 
    List<string>? field_list = null,
    bool includeAllProjects = false)
```

Queries for work items matching the specified criteria. Returns only the requested fields for each result.

**Parameters:**
- `query` - The query string to use while searching
- `order` - The field to order results by (default: "Created")
- `field_list` - List of fields to retrieve for each search result. If null, defaults to ["id"]. Use syntax like `['customFields.FieldName']` for custom fields
- `includeAllProjects` - When true, omits the automatic project.id filter so results span all projects. Default is false.

**Returns:** A `Result<WorkItem[]>` containing matching work items or error details. A query that matches nothing returns a successful, empty array. A query Polarion rejects (SOAP fault, e.g. invalid SQL) returns a failure carrying the server message.

**Remarks:** By default, automatically appends the project ID to the query. Set `includeAllProjects: true` to search across projects (e.g. for cross-project document references). For custom field retrieval, use the syntax: `field_list=['customFields.SomeField']`

---

### SearchWorkitemInBaselineAsync

```csharp
public async Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(
    string baselineRevision, 
    string query, 
    string order = "Created", 
    List<string>? field_list = null,
    bool includeAllProjects = false)
```

Queries for work items in a specific baseline revision. Returns only the requested fields for each result.

**Parameters:**
- `baselineRevision` - The revision number of the baseline to search in
- `query` - The query string to use while searching
- `order` - The field to order results by (default: "Created")
- `field_list` - List of fields to retrieve for each search result. If null, defaults to ["id"]. Use syntax like `['customFields.FieldName']` for custom fields
- `includeAllProjects` - When true, omits the automatic project.id filter so results span all projects. Default is false.

**Returns:** A `Result<WorkItem[]>` containing matching work items or error details. A query that matches nothing returns a successful, empty array. A query Polarion rejects (SOAP fault) returns a failure carrying the server message.

**Throws:** `PolarionClientException` if the operation fails

**Remarks:** Set `includeAllProjects: true` when querying baseline revisions of documents that contain cross-project work item references.

---

### GetWorkItemsByModuleAsync

```csharp
public async Task<Result<WorkItem[]>> GetWorkItemsByModuleAsync(
    string moduleTitle, 
    PolarionFilter filter, 
    string? moduleRevision = null)
```

Fetches work items from a specific module based on the specified criteria.

**Parameters:**
- `moduleTitle` - The title of the module to fetch data from
- `filter` - The filter criteria for work items (includes WorkItemFilter, Order, and Fields)
- `moduleRevision` - Optional revision identifier. If null, fetches from the latest revision

**Returns:** A `Result<WorkItem[]>` containing the work items or error details

**Remarks:** Filters out work items that don't have an outline number (i.e., not part of the module structure)

**Deprecated:** marked `[Obsolete]`. The `document.title` query only matches items owned by the document, so referenced and pinned items are omitted and results are not in document order. Use `GetModuleWorkItemsAsync` or `QueryWorkItemsInModuleAsync` instead.

---

### GetHierarchicalWorkItemsByModuleAsync

```csharp
public async Task<Result<SortedDictionary<string, SortedDictionary<string, WorkItem>>>> 
    GetHierarchicalWorkItemsByModuleAsync(
        string workItemPrefix, 
        string moduleTitle, 
        PolarionFilter filter, 
        string? moduleRevision = null)
```

Fetches work items from a module and organizes them into a hierarchical structure based on outline numbers.

**Parameters:**
- `workItemPrefix` - The prefix used for work item IDs
- `moduleTitle` - The title of the module to fetch data from
- `filter` - The filter criteria for work items
- `moduleRevision` - Optional revision identifier. If null, fetches from the latest revision

**Returns:** A `Result<SortedDictionary<string, SortedDictionary<string, WorkItem>>>` containing the hierarchical structure or error details

**Remarks:** Organizes items by parent heading and child items. Items with hyphens in their outline numbers are treated as children of the parent heading.

---

## Module Operations

### GetModulesInSpaceThinAsync

```csharp
public async Task<Result<ModuleThin[]>> GetModulesInSpaceThinAsync(string spaceName)
```

Retrieves all modules (documents) in a specific space.

**Parameters:**
- `spaceName` - Name of the space

**Returns:** A `Result<ModuleThin[]>` containing the modules sorted by title or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetModulesThinAsync

```csharp
public async Task<Result<ModuleThin[]>> GetModulesThinAsync(
    string? excludeSpaceNameContains = null, 
    string? titleContains = null)
```

Gets modules in the project that match the specified criteria.

**Parameters:**
- `excludeSpaceNameContains` - Optional filter to exclude modules whose folder name contains this string
- `titleContains` - Optional filter to include only modules whose title contains this string

**Returns:** A `Result<ModuleThin[]>` containing the filtered modules sorted by title or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetModuleByLocationAsync

```csharp
public async Task<Result<Module>> GetModuleByLocationAsync(string location)
```

Retrieves a module by its location path.

**Parameters:**
- `location` - The path of the module (e.g., "MySpace/MyDoc")

**Returns:** A `Result<Module>` containing the module or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetModuleByUriAsync

```csharp
public async Task<Result<Module>> GetModuleByUriAsync(string uri)
```

Retrieves a module by its URI.

**Parameters:**
- `uri` - The URI of the module

**Returns:** A `Result<Module>` containing the module or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetModuleWorkItemUrisAsync

```csharp
public async Task<Result<string[]>> GetModuleWorkItemUrisAsync(
    string moduleUri, 
    string? parentWorkItemUri = null, 
    bool deep = true)
```

Gets URIs of all work items in a module at the specified revision.

**Parameters:**
- `moduleUri` - The module URI (may include revision specifier, e.g., `moduleUri%revision`)
- `parentWorkItemUri` - Optional parent work item URI to filter children (default: null)
- `deep` - Whether to include external/linked items (default: true)

**Returns:** A `Result<string[]>` containing an array of work item URIs or error details

**Remarks:** This is useful for retrieving work items from a module at a specific historical revision. The module URI can include a revision suffix (e.g., `%200000`) to get work items as they existed at that point in time.

---

### GetModuleWorkItemsAsync

```csharp
public async Task<Result<ModuleWorkItem[]>> GetModuleWorkItemsAsync(
    string moduleUri,
    string? parentWorkItemUri = null,
    bool deep = true,
    List<string>? fields = null)
```

Gets the work items of a document (module) in document order, including items referenced from other documents or projects. Wraps the Polarion SOAP `getModuleWorkItems` call.

**Parameters:**
- `moduleUri` - The module URI. Append `%revision` to read the document as it was at that revision (for a baseline, use the baseline's base revision)
- `parentWorkItemUri` - Optional parent work item URI; when set, only its children are returned (default: null)
- `deep` - When true, returns the whole tree below the parent (or the whole document); when false, only direct children (default: true)
- `fields` - Optional list of fields to retrieve. Defaults to `id`, `type`, `title`, `description`, `status`, `outlineNumber`, `author`, `created`, `updated`

**Returns:** A `Result<ModuleWorkItem[]>` with one entry per document row, in document order. An empty document returns a successful, empty array. A service error returns a failure with the server message.

`ModuleWorkItem` properties:
- `WorkItem` - The work item with the requested fields. For pinned references the values are those at the pinned revision
- `Uri` - The work item URI as returned, including any `%revision` suffix
- `Id` - The work item ID (from the data, or parsed from the URI)
- `Revision` - The pinned revision parsed from the URI; empty when the row is not pinned
- `IsPinned` - True when `Revision` is set
- `IsUnresolvable` - True when Polarion marks the row unresolvable (e.g. a live reference to a deleted item). Such rows carry no field values

**Remarks:**
- Pinned references are returned with their pinned values, including items deleted after they were pinned. SQL/Lucene based queries return HEAD (or the document revision) values and miss deleted-but-pinned items.
- Keep the returned order. Do not re-sort by `outlineNumber`: referenced items carry the outline number of their home document.
- Unresolvable rows are returned so callers can report them; skip them if only resolvable content is wanted.
- This is the only method that exposes unresolvable rows. `QueryWorkItemsInModuleAsync` and `GetWorkItemsByModuleRevisionAsync` are built on it and drop them.

---

### QueryWorkItemsInModuleAsync

```csharp
public async Task<Result<WorkItem[]>> QueryWorkItemsInModuleAsync(
    string moduleFolder,
    string documentId,
    List<string>? itemTypes = null,
    string sort = "outlineNumber",
    List<string>? fields = null)
```

Gets the work items of a document at HEAD, in document order.

**Parameters:**
- `moduleFolder` - The module folder (space) path
- `documentId` - The document ID
- `itemTypes` - Optional list of work item type IDs to keep (filtered client-side; `type` is added to `fields` automatically)
- `sort` - Ignored. Kept for source compatibility; results are always in document order
- `fields` - Optional list of fields to retrieve. Defaults to `id`, `type`, `title`, `description`, `status`, `outlineNumber`

**Returns:** A `Result<WorkItem[]>` with the document's work items in document order. A document with no (matching) items returns a successful, empty array.

**Remarks:** Resolves the document with `GetModuleByLocationAsync`, then calls `GetModuleWorkItemsAsync`. Pinned references are returned with their pinned values and deleted-but-pinned items are included. Unresolvable rows are dropped; use `GetModuleWorkItemsAsync` to see them.

**Behavior change:** this method previously ran a SQL query against `POLARION.REL_MODULE_WORKITEM`. That returned HEAD values for pinned references, missed deleted-but-pinned items, honoured `sort`, and failed with "SQL query returned no results" for an empty document.

---

### GetWorkItemsByModuleRevisionAsync

```csharp
public async Task<Result<WorkItemWithRevisionInfo[]>> GetWorkItemsByModuleRevisionAsync(
    string moduleFolder,
    string documentId,
    string revision,
    List<string>? fields = null)
```

Gets the work items of a document as it was at a historical revision, in document order.

**Parameters:**
- `moduleFolder` - The module folder (space) path
- `documentId` - The document ID
- `revision` - The revision number. For a baseline, pass the baseline's base revision (see `QueryBaselinesAsync`)
- `fields` - Optional list of fields to retrieve. Defaults to `id`, `type`, `title`, `description`, `status`, `outlineNumber`, `author`, `created`, `updated`

**Returns:** A `Result<WorkItemWithRevisionInfo[]>` in document order:
- `WorkItem` - The item with the values it had in the document at that revision
- `Revision` - The item's pinned revision for pinned references, otherwise `revision`
- `SourceUri` - The item URI as returned (with `%revision` for pinned references)
- `IsHistorical` - Always true
- `HeadRevision` - Not populated

**Remarks:** Calls `GetModuleWorkItemsAsync` with `{moduleUri}%{revision}`. Unresolvable rows are dropped.

**Behavior change:** this method previously re-fetched items at the document revision via a baseline query. That returned wrong values for pinned references, omitted deleted-but-pinned items, and ordered results by ID.

---

## Baseline Operations

### QueryBaselinesAsync

```csharp
public async Task<Result<Baseline[]>> QueryBaselinesAsync(
    string query,
    string sort = "baseRevision")
```

Queries baselines (project and document baselines) with a Lucene query. Wraps the Polarion SOAP `queryBaselines` call.

**Parameters:**
- `query` - Lucene query over baselines, passed to Polarion unchanged. It is **not** scoped to the configured project; include e.g. `project.id:MyProject` to scope it
- `sort` - Sort field (default: `baseRevision`)

**Returns:** A `Result<Baseline[]>` with the matching baselines (`id`, `name`, `baseRevision`, `baseObjectURI`, ...). A query that matches nothing returns a successful, empty array.

**Remarks:** `baseObjectURI` is the project (project baseline) or the module (document baseline) the baseline was taken on. To read a document as it was at a baseline, call `GetModuleWorkItemsAsync($"{moduleUri}%{baseRevision}")` or `GetWorkItemsByModuleRevisionAsync(folder, docId, baseRevision)`.

---

### QueryModuleUrisInBaselineAsync

```csharp
public async Task<Result<string[]>> QueryModuleUrisInBaselineAsync(
    string baselineRevision,
    string query,
    string sort = "uri",
    int limit = -1)
```

Queries the URIs of documents (modules) as they existed at a baseline revision. Wraps the Polarion SOAP `queryModuleUrisInBaseline` call.

**Parameters:**
- `baselineRevision` - The baseline's base revision
- `query` - Lucene query over modules, passed to Polarion unchanged (not scoped to the configured project)
- `sort` - Sort field (default: `uri`)
- `limit` - Maximum number of results (default: -1 = all)

**Returns:** A `Result<string[]>` with the module URIs. A query that matches nothing returns a successful, empty array.

---

## Space Operations

### GetSpacesAsync

```csharp
public async Task<Result<List<string>>> GetSpacesAsync(string? excludeSpaceNameContains = null)
```

Retrieves all document spaces in the project.

**Parameters:**
- `excludeSpaceNameContains` - Optional filter to exclude spaces whose name contains this string

**Returns:** A `Result<List<string>>` containing the sorted space names or error details

---

## User Operations

### GetProjectUsersAsync

```csharp
public async Task<Result<Polarion.Generated.Project.User[]>> GetProjectUsersAsync(string projectId)
```

Gets the users explicitly assigned to a project (its member list), via `ProjectWebService.getProjectUsers`.

**Parameters:**
- `projectId` - The Polarion project ID

**Returns:** A `Result<Polarion.Generated.Project.User[]>` containing the project's assigned users or error details

---

### GetUsersAsync

```csharp
public async Task<Result<Polarion.Generated.Project.User[]>> GetUsersAsync()
```

Gets every user known to this Polarion instance, via `ProjectWebService.getUsers`.

**Returns:** A `Result<Polarion.Generated.Project.User[]>` containing all users or error details

**Remarks:** This SOAP operation takes no filter — callers needing to resolve a single user by email or id must filter the returned array themselves, and should cache the result rather than calling this per lookup.

---

## Markdown Export Operations

### ExportModuleToMarkdownAsync

```csharp
public async Task<Result<StringBuilder>> ExportModuleToMarkdownAsync(
    string workItemPrefix, 
    string moduleTitle, 
    PolarionFilter filter,
    Dictionary<string, string> workItemTypeToShortNameMap, 
    bool includeWorkItemIdentifiers = true, 
    string? revision = null)
```

Exports Polarion work items from a module to Markdown format asynchronously.

**Parameters:**
- `workItemPrefix` - The prefix used for work item IDs
- `moduleTitle` - The title of the module to export
- `filter` - The filter criteria for work items
- `workItemTypeToShortNameMap` - A dictionary mapping work item type IDs to short names
- `includeWorkItemIdentifiers` - Whether to include the work item identifiers in the Markdown output (default: true)
- `revision` - Optional revision identifier. If null, exports the latest revision

**Returns:** A `Result<StringBuilder>` containing the Markdown content or error details

---

### ExportModuleToMarkdownGroupedByHeadingAsync

```csharp
public async Task<Result<SortedDictionary<string, StringBuilder>>> 
    ExportModuleToMarkdownGroupedByHeadingAsync(
        int headingLevel, 
        string workItemPrefix, 
        string moduleTitle, 
        PolarionFilter filter,
        Dictionary<string, string> workItemTypeToShortNameMap, 
        bool includeWorkItemIdentifiers = true, 
        string? revision = null)
```

Exports Polarion work items grouped by heading level to Markdown format asynchronously.

**Parameters:**
- `headingLevel` - The heading level to group by
- `workItemPrefix` - The prefix used for work item IDs
- `moduleTitle` - The title of the module to export
- `filter` - The filter criteria for work items
- `workItemTypeToShortNameMap` - A dictionary mapping work item type IDs to short names
- `includeWorkItemIdentifiers` - Whether to include the work item identifiers in the Markdown output (default: true)
- `revision` - Optional revision identifier. If null, exports the latest revision

**Returns:** A `Result<SortedDictionary<string, StringBuilder>>` containing heading-grouped Markdown content or error details

---

### ConvertWorkItemToMarkdown

```csharp
public string ConvertWorkItemToMarkdown(
    string workItemId, 
    WorkItem? workItem, 
    string? errorMsgPrefix = null, 
    bool includeWorkItemIdentifiers = true)
```

Converts a Polarion work item to Markdown format.

**Parameters:**
- `workItemId` - The ID of the work item to convert
- `workItem` - The work item object to convert (can be null)
- `errorMsgPrefix` - An optional prefix to add to error messages
- `includeWorkItemIdentifiers` - Whether to include the metadata in the Markdown output (default: true)

**Returns:** A string containing the Markdown representation of the work item

**Remarks:** Handles HTML content conversion and Polarion-specific elements like math formulas and cross-references. Uses ReverseMarkdown for HTML to Markdown conversion.

---

## Revision Operations

### GetRevisionIdsAsync

```csharp
public async Task<Result<string[]>> GetRevisionIdsAsync(string uri)
```

Retrieves revision IDs for any persistent item by its URI.

**Parameters:**
- `uri` - The URI of the item

**Returns:** A `Result<string[]>` containing the revision IDs or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetRevisionsIdsByWorkItemIdAsync

```csharp
public async Task<Result<string[]>> GetRevisionsIdsByWorkItemIdAsync(string workItemId)
```

Retrieves revision identifiers for a work item by its ID.

**Parameters:**
- `workItemId` - The ID of the work item

**Returns:** A `Result<string[]>` containing the revision IDs or error details

**Throws:** `PolarionClientException` if the operation fails

---

### GetWorkItemRevisionsByIdAsync

```csharp
public async Task<Result<Dictionary<string, WorkItem>>> GetWorkItemRevisionsByIdAsync(
    string workItemId, 
    int maxRevisions = -1)
```

Retrieves revisions for a work item by its ID, returned as a dictionary keyed by revision ID.

**Parameters:**
- `workItemId` - The ID of the work item
- `maxRevisions` - Maximum number of revisions to return (newest to oldest). -1 returns all (default: -1)

**Returns:** A `Result<Dictionary<string, WorkItem>>` containing a dictionary where keys are revision IDs and values are the corresponding work items, or error details

**Throws:** `PolarionClientException` if the operation fails

**Remarks:** The dictionary structure allows direct lookup of work items by their revision ID without iteration.

---

### GetModuleRevisionsByLocationAsync

```csharp
public async Task<Result<Module[]>> GetModuleRevisionsByLocationAsync(
    string location, 
    int maxRevisions = -1)
```

Retrieves module revisions by location with configurable maximum revision limit.

**Parameters:**
- `location` - The path of the module (e.g., "MySpace/MyDoc")
- `maxRevisions` - Maximum number of revisions to return (newest to oldest). -1 returns all (default: -1)

**Returns:** A `Result<Module[]>` containing the module revisions or error details

**Throws:** `PolarionClientException` if the operation fails

---

## Configuration

### PolarionClientConfiguration

```csharp
public record PolarionClientConfiguration
{
    public string ServerUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ProjectId { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
```

Configuration record for initializing the Polarion client.

**Properties:**
- `ServerUrl` - The base URL of the Polarion server (e.g., "https://polarion.example.com")
- `Username` - The username for authentication
- `Password` - The password for authentication
- `ProjectId` - The ID of the Polarion project to work with
- `TimeoutSeconds` - The timeout in seconds for WCF service calls (default: 30)

---

## Properties

### TrackerService

```csharp
public TrackerWebService TrackerService { get; }
```

Gets the underlying WCF TrackerWebService client used for communication with Polarion.

---

### ProjectService

```csharp
public Polarion.Generated.Project.ProjectWebService ProjectService { get; }
```

Gets the underlying WCF ProjectWebService client used for project membership and user lookups.

---

## Notes

- All async methods return a `Result<T>` type that indicates success or failure
- Methods marked with `[RequiresUnreferencedCode]` require reflection and may not work with trimmed assemblies
- HTML content in work item descriptions is automatically converted to Markdown
- Polarion-specific elements (math formulas, cross-references) are handled during conversion
- All queries automatically include the configured project ID filter
