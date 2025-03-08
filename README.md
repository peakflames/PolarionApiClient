# PolarionApiClient

A .NET client library for accessing Polarion Requirements Management System via SOAP API.

## Installation

```sh
dotnet add package Polarion
```

## Usage

### Creating the Client

```csharp
var config = new PolarionClientConfiguration
{
    ServerUrl = "https://your-polarion-server",
    Username = "your-username",
    Password = "your-password",
    ProjectId = "YourProject",
    TimeoutSeconds = 60
};

var polarionClient = await PolarionClient.CreateAsync(config);
```

### Working with Work Items

```csharp
// Get a specific work item
var workItemResult = await polarionClient.GetWorkItemByIdAsync("REQ-123");
if (workItemResult.IsSuccess)
{
    var workItem = workItemResult.Value;
    // Work with the item
}

// Query work items with specific fields
var query = "type:requirement AND status:approved";
var fields = new List<string> { "id", "title", "status", "customFields.myCustomField" };
var searchResult = await polarionClient.SearchWorkitem(query, "Created", fields);

// Query work items in a baseline
var baselineResult = await polarionClient.SearchWorkitemInBaseline(
    "myBaseline", 
    "type:requirement", 
    "Created"
);
```

### Working with Documents

```csharp
// Get all documents in a space
var documentsResult = await polarionClient.GetDocumentsInSpace("MySpace");
if (documentsResult.IsSuccess)
{
    foreach (var document in documentsResult.Value)
    {
        // Work with document
    }
}
```

## Features

- Access Polarion work items via SOAP API
- Query work items with Lucene query language
- Support for baseline queries
- Document/Module access capabilities
- Custom field support
- Strongly-typed models
- Built on .NET 8.0
- Automatic session management
- Secure cookie handling
- Configurable timeouts

## Query Examples

### Basic Queries
```csharp
// Search for approved requirements
"type:requirement AND status:approved"

// Search for items modified in last week
"updated:[NOW-7DAYS TO NOW]"
```

### Using Custom Fields
When retrieving custom fields, use the following syntax in the field_list:
```csharp
var fields = new List<string> { "customFields.myCustomField" };
```

## Error Handling

The client uses a Result pattern for error handling. All operations return a `Result<T>` that can be checked for success:

```csharp
var result = await polarionClient.GetWorkItemByIdAsync("REQ-123");
if (result.IsSuccess)
{
    var workItem = result.Value;
    // Work with the item
}
else
{
    var errorMessage = result.Error;
    // Handle error
}
```

## License

MIT
