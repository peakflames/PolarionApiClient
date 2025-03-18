# Polarion Changelog

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
