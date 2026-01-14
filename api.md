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
  - [Space Operations](#space-operations)
    - [GetSpacesAsync](#getspacesasync)
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
    List<string>? field_list = null)
```

Queries for work items matching the specified criteria. Returns only the requested fields for each result.

**Parameters:**
- `query` - The query string to use while searching
- `order` - The field to order results by (default: "Created")
- `field_list` - List of fields to retrieve for each search result. If null, defaults to ["id"]. Use syntax like `['customFields.FieldName']` for custom fields

**Returns:** A `Result<WorkItem[]>` containing matching work items or error details

**Remarks:** Automatically appends the project ID to the query. For custom field retrieval, use the syntax: `field_list=['customFields.SomeField']`

---

### SearchWorkitemInBaselineAsync

```csharp
public async Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(
    string baselineRevision, 
    string query, 
    string order = "Created", 
    List<string>? field_list = null)
```

Queries for work items in a specific baseline revision. Returns only the requested fields for each result.

**Parameters:**
- `baselineRevision` - The revision number of the baseline to search in
- `query` - The query string to use while searching
- `order` - The field to order results by (default: "Created")
- `field_list` - List of fields to retrieve for each search result. If null, defaults to ["id"]. Use syntax like `['customFields.FieldName']` for custom fields

**Returns:** A `Result<WorkItem[]>` containing matching work items or error details

**Throws:** `PolarionClientException` if the operation fails

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

## Notes

- All async methods return a `Result<T>` type that indicates success or failure
- Methods marked with `[RequiresUnreferencedCode]` require reflection and may not work with trimmed assemblies
- HTML content in work item descriptions is automatically converted to Markdown
- Polarion-specific elements (math formulas, cross-references) are handled during conversion
- All queries automatically include the configured project ID filter
