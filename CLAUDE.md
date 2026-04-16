# PolarionApiClient - Claude Code Guidelines

## Build & Test

```bash
dotnet build src/PolarionApiClient.sln
dotnet test src/PolarionApiClient.sln
```

## Architecture

### Partial Class Pattern
Each public method lives in its own file:
- Location: `src/Polarion/Client/`
- Naming: `PolarionClient_MethodName.cs`
- Main class: `PolarionClient.cs` (properties, fields, constructor)
- Interface: `IPolarionClient.cs`

### Three-Way Sync
Every public method change requires updates to all three:
1. `src/Polarion/Client/PolarionClient_MethodName.cs` — implementation
2. `src/Polarion/Client/IPolarionClient.cs` — interface signature
3. `api.md` — API documentation (root directory)

## Change Checklist

- [ ] Update implementation file
- [ ] Update interface signature
- [ ] Add/update tests in `src/Polarion.Tests/Integration/PolarionClientTests.cs`
- [ ] Update `CHANGELOG.md` (add entry under current version)
- [ ] Update `api.md`
- [ ] Bump version in `src/Polarion/Polarion.csproj` if needed
- [ ] Run `dotnet build src/PolarionApiClient.sln` to verify

## Coding Standards

### Method Signatures
```csharp
[RequiresUnreferencedCode("Uses WCF services which require reflection")]
public async Task<Result<T>> MethodNameAsync(params, string? optional = null)
```

### Error Handling
- Use `Result<T>` from FluentResults
- Return `Result.Fail("message")` for expected failures
- Throw `PolarionClientException` for unexpected errors

### XML Documentation (required on all public methods)
```csharp
/// <summary>Brief description</summary>
/// <param name="paramName">Description</param>
/// <returns>Description of return value</returns>
/// <exception cref="PolarionClientException">When it throws</exception>
/// <remarks>Additional context, performance notes</remarks>
```

### Optional Parameters
- `string? paramName = null` for optional strings
- `int paramName = -1` for optional counts (-1 = all)

### Revision Support Pattern
- Optimize for the no-revision (current) case
- Two-call pattern: get URI → get by URI + revision
- Provide clear error messages for revision not found

## Testing

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
```

- All tests in `src/Polarion.Tests/Integration/PolarionClientTests.cs`
- Use FluentAssertions
- Test both success and failure paths

## Version Management

- **Patch (0.0.X)**: Bug fixes, minor improvements
- **Minor (0.X.0)**: New features, backward compatible
- **Major (X.0.0)**: Breaking changes

Version is in `src/Polarion/Polarion.csproj` → `<Version>`.

## Releases

Releases are performed via the **Release to NuGet** GitHub Actions workflow — see `CONTRIBUTING.md` for full instructions.

**CRITICAL**: Never stage, commit, or touch `src/Polarion.Tests/appsettings.test.json` — it contains sensitive credentials.

## CHANGELOG.md Format

```markdown
## X.Y.Z

- **Breaking Change**: Description (if applicable)
- Feature: Description
- Fix: Description
```
