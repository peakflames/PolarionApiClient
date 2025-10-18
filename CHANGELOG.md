# Polarion Changelog

## 0.3.2

- **Breaking Change**: `GetWorkItemRevisionsByIdAsync` now returns `Dictionary<string, WorkItem>` instead of `WorkItem[]`, where the key is the revision ID

## 0.3.1

- **Interface**: Updated `IPolarionClient` interface to include all missing methods (`GetModuleByLocationAsync`, `GetModuleByUriAsync`, `GetRevisionIdsAsync`, `GetRevisionsIdsByWorkItemIdAsync`, `GetWorkItemRevisionsByIdAsync`, `GetModuleRevisionsByLocationAsync`)

## 0.3.0

- Add `GetWorkItemRevisionsByIdAsync` and `GetRevisionIdsByWorkItemIdAsync`
- Add `GetRevisionIdsAsync`
- Add `GetModuleByUriAsync` and `GetModuleByLocationAsync` and `GetModuleRevisionsByLocationAsync`
- ci: add GitHub Actions workflow for building and publishing to NuGet

## 0.2.1

- make module filtering case-insensitive in `GetModulesThinAsync`

## 0.2.0

- Updated `ExportModuleToMarkdownAsync` by adding parameter to control visibility of work item **identifiers**
- Updated `ExportModuleToMarkdownGroupedByHeadingAsync` by adding parameter to control visibility of work item **identifiers**
- Adding `ConvertWorkItemToMarkdown` method
- Add LaTeX and cross-reference support in Polarion HTML to Markdown conversion

## 0.1.0

- Added `GetWorkItemsByModuleAsync`
- Added `GetHierarchicalWorkItemsByModuleAsync`
- Added `ExportModuleToMarkdownAsync`
- Added `ExportModuleToMarkdownGroupedByHeadingAsync`

## 0.0.9

- Improve null handling in GetModulesThinAsync method

## 0.0.8

- Renamed GetDocumentsInSpaceAsync to `GetModulesInSpaceThinAsync`
- Added `GetModulesThinAsync` method

## 0.0.7

- Added GetDocumentSpacesAsync method
- Refactored Client class into partial classes

## 0.0.6

- Update PolarionClientConfiguration type to no longer need requred fields. This is to support more use cases in json serialization.

## 0.0.5

- Added support for trimming
- Added RequiresUnreferencedCode attributes to API methods
- Added TrimmerRoots.xml for reflection support
- Updated documentation with trimming guidance

## 0.0.4

- Fix bug in SearchWorkitemAsync and SearchWorkitemInBaselineAsync

## 0.0.3

- Add Suffix to all IPolarionClient methods

## 0.0.2

- PolarionClient.CreateAsync() now returns a Result<PolarionClient> type.

## 0.0.1

- Initial release
    - Added `PolarionClient` class
    - Added `IPolarionClient` interface
    - Added `PolarionClientConfiguration` class
    - IPolarionClient implementation
        - GetWorkItemByIdAsync
        - SearchWorkitem
        - SearchWorkitemInBaseline
        - GetDocumentsInSpace
