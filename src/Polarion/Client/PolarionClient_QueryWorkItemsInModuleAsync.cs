namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Default fields for SQL-based work item queries.
    /// Note: "uri" is not a valid field key for SQL queries - it's available via C_URI in the query but not as a field.
    /// </summary>
    private static readonly List<string> SqlQueryWorkItemFields =
    [
        "id", "type", "title", "description", "status", "outlineNumber"
    ];

    /// <summary>
    /// Queries work items using SQL against POLARION.REL_MODULE_WORKITEM relationship.
    /// </summary>
    /// <remarks>
    /// Uses direct SQL query against Polarion database schema:
    /// - POLARION.WORKITEM (item)
    /// - POLARION.MODULE (doc)
    /// - POLARION.PROJECT (proj)
    /// - POLARION.REL_MODULE_WORKITEM (relationship)
    /// </remarks>
    /// <param name="moduleFolder">The module folder path</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="itemTypes">Optional list of work item types to filter</param>
    /// <param name="sort">Sort field (default: outlineNumber)</param>
    /// <param name="fields">Optional list of fields to retrieve</param>
    /// <returns>Array of work items in the module</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> QueryWorkItemsInModuleAsync(
        string moduleFolder,
        string documentId,
        List<string>? itemTypes = null,
        string sort = "outlineNumber",
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

        // Use SQL-compatible fields (without "uri")
        var fieldList = fields ?? SqlQueryWorkItemFields;

        // Build item type filter if provided
        var itemTypeFilter = string.Empty;
        if (itemTypes != null && itemTypes.Count > 0)
        {
            var typeList = string.Join(", ", itemTypes.Select(t => $"'{t}'"));
            itemTypeFilter = $"AND item.C_TYPE IN ({typeList})";
        }

        // Build SQL query against Polarion database schema
        var sqlQuery = $@"SQL:(
            SELECT item.* FROM POLARION.WORKITEM item, POLARION.MODULE doc, POLARION.PROJECT proj
            WHERE proj.C_ID = '{_config.ProjectId}'
                AND doc.FK_PROJECT = proj.C_PK
                AND doc.C_MODULEFOLDER = '{moduleFolder}'
                AND doc.C_ID = '{documentId}'
                {itemTypeFilter}
                AND EXISTS 
                (
                    SELECT rel1.* 
                    FROM POLARION.REL_MODULE_WORKITEM rel1
                    WHERE rel1.FK_URI_MODULE = doc.C_URI AND rel1.FK_URI_WORKITEM = item.C_URI
                )
        )";

        try
        {
            // Use the raw queryWorkItemsAsync directly to avoid the automatic project.id filter
            // that SearchWorkitemAsync adds (since our SQL query handles project filtering)
            var result = await _trackerClient.queryWorkItemsAsync(new(sqlQuery, sort, [.. fieldList]));

            if (result?.queryWorkItemsReturn is null)
            {
                return Result.Fail<WorkItem[]>("SQL query returned no results");
            }

            return Result.Ok(result.queryWorkItemsReturn);
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute SQL query for module work items. {ex.Message}");
        }
    }
}
