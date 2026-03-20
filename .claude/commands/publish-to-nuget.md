Execute the complete NuGet release process for PolarionApiClient.

## CRITICAL WARNING

**NEVER commit, stage, reset, checkout, or touch `src/Polarion.Tests/appsettings.test.json`** — this file contains sensitive credentials. Skip it in all git operations.

## Step 1: Pre-flight Checks

Run `git status` and review the output. If there are uncommitted changes (excluding `appsettings.test.json`), ask the user whether those changes should be committed before proceeding.

Confirm we are on the `develop` branch. If not, stop and ask the user.

## Step 2: Determine the New Version

Read the current version from `src/Polarion/Polarion.csproj` (the `<Version>` element). Ask the user what the new release version should be and what changes to summarize in the changelog entry.

## Step 3: Update Version and Changelog

Update `<Version>X.X.X</Version>` in `src/Polarion/Polarion.csproj` to the new version.

Add a new entry at the top of `CHANGELOG.md`:
```markdown
## X.X.X

- [changes provided by user]
```

## Step 4: Build Validation

Run:
```bash
dotnet clean src/PolarionApiClient.sln && dotnet build src/PolarionApiClient.sln
```

If the build fails, stop and report the errors to the user. Do not proceed.

## Step 5: Commit the Release Changes

Stage only the changed files (never `appsettings.test.json`):
```bash
git add src/Polarion/Polarion.csproj CHANGELOG.md
```

Commit:
```bash
git commit -m "Release vX.X.X: [brief description]"
```

## Step 6: Merge to Main

**Confirm with the user before proceeding** — the following steps push to remote and are not easily reversible.

```bash
git switch main
git merge develop --no-ff -m "Release vX.X.X: [brief description]"
git push origin main
```

## Step 7: Create and Push Tag

```bash
git tag -a vX.X.X -m "Release vX.X.X: [brief description]"
git push origin vX.X.X
```

## Step 8: Return to Develop and Bump Version

Switch back and bump to the next development version (ask the user if unsure):
```bash
git switch develop
```

Update `<Version>` in `src/Polarion/Polarion.csproj` to the next version (e.g. next patch or minor).

Add a placeholder at the top of `CHANGELOG.md`:
```markdown
## X.X.X

- TBD
```

Stage and commit:
```bash
git add src/Polarion/Polarion.csproj CHANGELOG.md
git commit -m "(chore) bump version to vX.X.X (develop)"
git push origin develop
```

## Step 9: Verify

Confirm the tag is visible on GitHub and that the NuGet publish GitHub Actions workflow has triggered. The package should appear on NuGet.org within a few minutes.
