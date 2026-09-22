# Contributing to PolarionApiClient

## Team Structure

| Role | Group | Capabilities |
|---|---|---|
| Maintainer | team-sixseven | Merge PRs, trigger releases via GitHub Actions |
| Admin | Repository admins | All of the above plus branch protection and secret management |

---

## Development Workflow

### Branch Strategy

| Branch | Purpose |
|---|---|
| `develop` | Active development — all PRs target this branch |
| `main` | Release-only — always reflects the latest published NuGet package |

Never commit directly to `main`. All changes flow through PRs into `develop`, then through the release workflow into `main`.

### Making Changes

1. Create a feature branch from `develop`
2. Open a PR targeting `develop`
3. One approving review from team-sixseven is required to merge

Feature PRs must not touch `<Version>` in `src/Polarion/Polarion.csproj` or `CHANGELOG.md` — the release workflow owns both, authoring the changelog entry from its `changelog_entry` input at release time. A PR that bumps the version or hand-edits the changelog collides with the placeholder the previous release left behind.

### Critical: Never Touch This File

**`src/Polarion.Tests/appsettings.test.json`** contains sensitive credentials. Never stage, commit, or push this file.

---

## Release Process (Team-Sixseven Maintainers)

Releases are self-service via GitHub Actions — no direct push access to `main` is required.

### Pre-Release Checklist

- [ ] All intended changes are merged to `develop`
- [ ] `develop` builds cleanly locally: `dotnet build src/PolarionApiClient.sln`
- [ ] You know the release version (`new_version`) and the next development version (`next_dev_version`, usually the next patch)
- [ ] You've composed the changelog entry: run `git log <last-tag>..develop --oneline` to see what actually landed since the last release, and write one `;`-delimited bullet per merged PR
- [ ] The `## X.X.X` section already on `develop` reading `- TBD` is expected — it was left by the previous release's dev-version bump. `changelog_entry` fills it in; it is not dropped or skipped.

### Triggering a Release

1. Go to the repository on GitHub
2. Navigate to **Actions → Release to NuGet**
3. Click **Run workflow** (leave branch as `main`)
4. Fill in the three inputs:

| Input | Description | Example |
|---|---|---|
| `new_version` | Version to release | `0.4.0` |
| `changelog_entry` | Release summary. Separate multiple bullets with `;` — one per merged PR | `Feature: Added GetLinkedWorkItemsAsync; Fix: Lift WCF MaxReceivedMessageSize cap to int.MaxValue (#8)` |
| `next_dev_version` | Next development version | `0.4.1` |

5. Click **Run workflow** and monitor the Actions tab

### What the Workflow Does

The workflow mirrors the full manual release process end-to-end:

1. Updates `<Version>` in `src/Polarion/Polarion.csproj` on `develop`
2. Writes the `changelog_entry` into the `## X.X.X` section of `CHANGELOG.md` on `develop` — filling in the `- TBD` placeholder left by the previous release, or prepending a new section if none exists
3. Aborts if the `## X.X.X` section still reads `- TBD` after that (e.g. a blank `changelog_entry`), before anything is built, committed, or published
4. Runs `dotnet build` — aborts on failure
5. Commits and pushes the release changes to `develop`
6. Merges `develop → main` (no fast-forward)
7. Creates and pushes tag `vX.X.X`
8. Merges `main` back into `develop` (normally a fast-forward, since `develop`'s tip is already a parent of the release merge commit) so `develop` never diverges from `main`
9. Bumps `develop` to `next_dev_version` with a TBD changelog placeholder
10. Packs and publishes the NuGet package to NuGet.org
11. Creates a GitHub Release

The workflow is idempotent — if it fails partway through and is re-run with the same inputs, it safely skips steps that were already completed.

### Verifying a Release

After the workflow completes:

- Tag `vX.X.X` is visible under [Releases](https://github.com/peakflames/PolarionApiClient/releases)
- `develop` is bumped to `next_dev_version` with a `- TBD` placeholder in `CHANGELOG.md`
- `develop` contains `main`: `git merge-base --is-ancestor main develop` exits `0`
- Package appears on NuGet.org within a few minutes: `https://www.nuget.org/packages/Polarion/X.X.X`

---

## Admin Reference

### Required Repository Secret

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY` | API key for publishing to NuGet.org — must have push access to the `Polarion` package |

### Branch Protection Configuration

The release workflow depends on the `github-actions` app being allowed to bypass PR review requirements on both branches. This is configured via `bypass_pull_request_allowances` in branch protection — **not** via push restrictions, which do not work with `GITHUB_TOKEN`.

#### `main`

```json
{
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": true,
    "bypass_pull_request_allowances": { "apps": ["github-actions"] }
  },
  "restrictions": null
}
```

#### `develop`

```json
{
  "enforce_admins": false,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": true,
    "bypass_pull_request_allowances": { "apps": ["github-actions"] }
  },
  "restrictions": null
}
```

If branch protection is ever reset and the release workflow starts failing with permission errors, re-apply the `github-actions` bypass using the `gh` CLI:

```bash
gh api repos/peakflames/PolarionApiClient/branches/main/protection \
  --method PUT \
  --header "Accept: application/vnd.github+json" \
  --input - << 'EOF'
{
  "required_status_checks": {"strict": true, "checks": []},
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "dismissal_restrictions": {"users": [], "teams": ["team-sixseven"]},
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": true,
    "required_approving_review_count": 1,
    "require_last_push_approval": false,
    "bypass_pull_request_allowances": {"users": [], "teams": [], "apps": ["github-actions"]}
  },
  "restrictions": null,
  "required_linear_history": false,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_conversation_resolution": false
}
EOF
```

Run the same command for `develop`, replacing `main` with `develop` and setting `"enforce_admins": false`.
