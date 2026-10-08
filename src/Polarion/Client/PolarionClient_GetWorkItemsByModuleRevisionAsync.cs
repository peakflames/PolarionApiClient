namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Default fields to retrieve when querying work items.
    /// Covers all fields consumed by the REST API WorkItemAttributes response model.
    /// </summary>
    private static readonly List<string> DefaultWorkItemFields =
    [
        "id", "type", "title", "description", "status", "outlineNumber",
        "author", "created", "updated"
    ];

    /// <summary>
    /// Queries work items from a module at a specific historical revision, in document order.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    ///   1. Build the module URI from <c>moduleFolder</c> and <c>documentId</c> (no server round trip)
    ///   2. Call <see cref="GetModuleWorkItemsAsync"/> on <c>{moduleUri}%{revision}</c>
    ///   3. Drop unresolvable rows and wrap the rest as <see cref="WorkItemWithRevisionInfo"/>
    ///
    /// Each item carries the values it had in the document at that revision: pinned references are
    /// returned at their pinned revision (<see cref="WorkItemWithRevisionInfo.Revision"/> is the pinned
    /// revision), other items at <paramref name="revision"/>. Pinned references to items deleted after
    /// pinning are included. <see cref="WorkItemWithRevisionInfo.IsHistorical"/> is always true and
    /// <see cref="WorkItemWithRevisionInfo.HeadRevision"/> is not populated.
    ///
    /// For a baseline, pass the baseline's base revision. Use <see cref="GetModuleWorkItemsAsync"/>
    /// directly to see unresolvable rows.
    ///
    /// When no rows come back, the module is looked up at <c>{moduleUri}%{revision}</c>. If Polarion
    /// reports it unresolvable (a mistyped document ID, or a revision before the document existed),
    /// the result is a failure, "Document not found at revision N". An existing empty document returns
    /// a successful, empty array.
    ///
    /// Behavior change: previously items were re-fetched at the document revision via a baseline
    /// query, which returned wrong values for pinned references and omitted deleted-but-pinned items.
    /// Results are now in document order rather than ID order.
    /// </remarks>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="revision">The revision number. Surrounding whitespace is trimmed; anything other than
    /// digits fails without calling the server</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items with revision information, in document order</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItemWithRevisionInfo[]>> GetWorkItemsByModuleRevisionAsync(
        string moduleFolder,
        string documentId,
        string revision,
        List<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(moduleFolder))
        {
            return Result.Fail("Module folder cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(documentId))
        {
            return Result.Fail("Document ID cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(revision))
        {
            return Result.Fail("Revision cannot be null or empty");
        }

        // The revision becomes part of the module URI (moduleUri%revision); anything but digits would
        // change what is requested.
        revision = revision.Trim();
        if (!revision.All(char.IsAsciiDigit))
        {
            return Result.Fail($"Revision must be a revision number (digits only): '{revision}'");
        }

        // Step 1: Build the module URI
        var moduleUri = BuildModuleUri(moduleFolder, documentId);

        // Step 2: Read the document rows at the requested revision
        var revisionUri = $"{moduleUri}%{revision}";
        var rowsResult = await GetModuleWorkItemsAsync(revisionUri, null, true, fields);
        if (rowsResult.IsFailed)
        {
            return Result.Fail<WorkItemWithRevisionInfo[]>(
                $"Failed to get work items at revision {revision}: {rowsResult.Errors.First().Message}");
        }

        // No rows: an empty document is a valid, empty result; no document at that revision is not.
        if (rowsResult.Value.Length == 0)
        {
            var exists = await EnsureModuleResolvableAsync(
                revisionUri, $"Document not found at revision {revision}: '{moduleFolder}/{documentId}'");
            if (exists.IsFailed)
            {
                return Result.Fail<WorkItemWithRevisionInfo[]>(exists.Errors);
            }
        }

        // Step 3: Wrap results — all items are historical by definition (revision query)
        var finalWorkItems = rowsResult.Value
            .Where(row => !row.IsUnresolvable)
            .Select(row => new WorkItemWithRevisionInfo
            {
                WorkItem = row.WorkItem,
                Revision = row.IsPinned ? row.Revision : revision,
                HeadRevision = string.Empty,
                IsHistorical = true,
                SourceUri = row.Uri
            })
            .ToArray();

        return Result.Ok(finalWorkItems);
    }
}
