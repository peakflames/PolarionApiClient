# Polarion API Client - Developer Guidelines

## Project Architecture

### Partial Class Pattern
- Each public method = one file in `src/Polarion/Client/`
- Naming: `PolarionClient_MethodName.cs`
- Main class: `PolarionClient.cs` (properties, fields, base implementation)

### Three-Way Sync Required
Every public method change requires updates to:
1. Implementation: `src/Polarion/Client/PolarionClient_MethodName.cs`
2. Interface: `src/Polarion/Client/IPolarionClient.cs`
3. Documentation: `api.md` (root directory)

## Development Workflow

### Standard Change Checklist
- [ ] Update implementation file
- [ ] Update interface signature
- [ ] Add/update tests in `src/Polarion.Tests/Integration/PolarionClientTests.cs`
- [ ] Update CHANGELOG.md (add entry under current version)
- [ ] Update api.md with new signatures and descriptions
- [ ] Update version in `src/Polarion/Polarion.csproj` if needed
- [ ] Run `dotnet build src/PolarionApiClient.sln` to verify

### Version Management
- **Patch (0.0.X)**: Bug fixes, minor improvements
- **Minor (0.X.0)**: New features, backward compatible
- **Major (X.0.0)**: Breaking changes
- Mark breaking changes clearly in CHANGELOG

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
- Provide specific, actionable error messages

### XML Documentation
Required on all public methods:
```csharp
/// <summary>Brief description</summary>
/// <param name="paramName">Description</param>
/// <returns>Description of return value</returns>
/// <exception cref="PolarionClientException">When it throws</exception>
/// <remarks>Additional context, performance notes</remarks>
```

### Performance Optimization
- Minimize API calls (prefer single call when possible)
- Use fast path for default/common scenarios
- Document performance characteristics in remarks

### Backward Compatibility
- Use optional parameters for new features
- Default values maintain existing behavior
- Mark breaking changes explicitly

## Testing

### Test Requirements
- Add test for new functionality
- Update existing tests if signatures change
- Test both success and failure paths
- Use FluentAssertions for readable assertions

### Test Naming
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
```

## Documentation

### CHANGELOG.md Format
```markdown
## X.Y.Z

- **Breaking Change**: Description (if applicable)
- Feature: Description
- Fix: Description
```

### api.md Format
- Keep method signatures current
- Include all parameters with descriptions
- Document return types clearly
- Add remarks for performance/usage notes

## Common Patterns

### Async Methods
- All I/O operations are async
- Use `await` consistently
- Return `Task<Result<T>>`

### Optional Parameters
- Use `string? paramName = null` for optional strings
- Use `int paramName = -1` for optional counts (where -1 = all)
- Document default behavior clearly

### Revision Support
- When adding revision support, optimize for no-revision case
- Use two-call pattern: get URI → get by URI + revision
- Provide clear error messages for revision not found

## File Organization

```
src/Polarion/
├── Client/
│   ├── PolarionClient.cs              # Main class
│   ├── IPolarionClient.cs             # Interface
│   ├── PolarionClient_MethodName.cs   # One per method
│   └── ...
├── Connection/                         # WCF setup
├── Entities/                          # Custom types
├── Generated/                         # WSDL-generated code
└── Polarion.csproj                    # Version here

src/Polarion.Tests/
└── Integration/
    └── PolarionClientTests.cs         # All integration tests

Root:
├── CHANGELOG.md                       # Version history
├── api.md                            # API documentation
└── README.md                         # Getting started
```

## Quick Reference

### Adding a New Method
1. Create `PolarionClient_NewMethod.cs`
2. Add signature to `IPolarionClient.cs`
3. Add test to `PolarionClientTests.cs`
4. Update `CHANGELOG.md`
5. Update `api.md`
6. Build and verify

### Modifying Existing Method
1. Update implementation file
2. Update interface if signature changed
3. Update/add tests
4. Update `CHANGELOG.md`
5. Update `api.md`
6. Consider version bump if breaking
7. Build and verify

### Breaking Change Checklist
- [ ] Increment major or minor version
- [ ] Mark as **Breaking Change** in CHANGELOG
- [ ] Update all affected documentation
- [ ] Consider migration path for users
- [ ] Update tests comprehensively
