
# PolarionApiClient

A .NET client library for accessing Polarion Requirements Management System via SOAP API.

## Installation

```sh
dotnet add package Polarion
```

## Usage

### Setup with Dependency Injection

```csharp
services.AddPolarionClient(config =>
{
    config.ServerUrl = "https://your-polarion-server";
    config.Username = "your-username";
    config.Password = "your-password";
    config.TimeoutSeconds = 60;
});
```

### Direct Usage Example

```csharp
// Resolve the client from DI
var polarionClient = serviceProvider.GetRequiredService<IPolarionClient>();

// Get a specific work item
var workItem = await polarionClient.GetWorkItemByIdAsync("YourProject", "REQ-123");

// Query work items
var requirements = await polarionClient.QueryWorkItemsAsync("type:requirement AND status:approved");
```

## Features

- Access Polarion work items via SOAP API
- Query work items with Lucene query language
- Strongly-typed models
- Built on latest .NET standards
- DI-friendly implementation

## License

MIT
