# Publish to NuGet Workflow

This workflow handles the complete release process for the PolarionApiClient NuGet package, including version updates, git operations, and tagging.

## ⚠️ CRITICAL WARNING

**NEVER commit, add, reset, checkout, discard, or modify `src/Polarion.Tests/appsettings.test.json`** - this file contains sensitive credentials that must be protected at all costs. This file should never be touched in any way during the release process.

## Prerequisites

- You should be on the `develop` branch
- The `develop` branch should contain all the changes you want to release

## Step 1: Pre-flight Checks

Use the 'execute_command' tool to check git status:
```bash
git status
```

If there are uncommitted changes (excluding `appsettings.test.json`), use the 'ask_followup_question' tool to ask the user if they should be committed before proceeding with the release.

## Step 2: Update Version and Changelog

Use the 'read_file' tool to check the current version in `src/Polarion/Polarion.csproj`:
- Look for the `<Version>` element

Use the 'replace_in_file' tool to update the version (replace X.X.X with the new version):
```xml
<Version>X.X.X</Version>
```

Use the 'replace_in_file' tool to add a new version entry at the top of `CHANGELOG.md`:
```markdown
## X.X.X

- [List of changes]
```

## Step 3: Build Validation

Use the 'execute_command' tool to clean the solution:
```bash
dotnet clean src/PolarionApiClient.sln
```

Use the 'execute_command' tool to build the solution:
```bash
dotnet build src/PolarionApiClient.sln
```

Verify the build succeeds before proceeding.

## Step 4: Merge to Main

Use the 'execute_command' tool to switch to main branch:
```bash
git switch main
```

Use the 'execute_command' tool to merge develop with no fast-forward:
```bash
git merge develop --no-ff -m "Release vX.X.X: [brief description of changes]"
```

Use the 'execute_command' tool to push to remote:
```bash
git push origin main
```

## Step 5: Create and Push Tag

Use the 'execute_command' tool to create an annotated tag:
```bash
git tag -a vX.X.X -m "Release vX.X.X: [brief description of changes]"
```

Use the 'execute_command' tool to push the tag to remote:
```bash
git push origin vX.X.X
```

## Step 6: Return to Develop and Bump Version

Use the 'execute_command' tool to return to develop branch:
```bash
git switch develop
```

Use the 'replace_in_file' tool to bump the version to the next minor version in `src/Polarion/Polarion.csproj`:
```xml
<Version>X.X.X</Version>
```

Use the 'replace_in_file' tool to add a placeholder for the next version at the top of `CHANGELOG.md`:
```markdown
## X.X.X

- TBD
```

Use the 'execute_command' tool to stage the changes:
```bash
git add src/Polarion/Polarion.csproj CHANGELOG.md
```

Use the 'execute_command' tool to commit the version bump:
```bash
git commit -m "(chore) bump version to vX.X.X (develop)"
```

Use the 'execute_command' tool to push to remote:
```bash
git push origin develop
```

## Step 7: Verification

- Verify the tag appears on GitHub
- GitHub Actions will show the NuGet publish workflow status
- The package should be available on NuGet.org within a few minutes

## Example Usage

For version 0.3.2 with API documentation updates:
- Update version to `0.3.2`
- Changelog entry: `- Enhanced API documentation and interface completeness`
- Merge message: `"Release v0.3.2: Enhanced API documentation and interface completeness"`
- Tag: `v0.3.2`
