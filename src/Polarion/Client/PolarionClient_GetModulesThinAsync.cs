namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets modules in the project that match the specified criteria
    /// </summary>
    /// <param name="excludeSpaceNameContains">Optional filter to exclude modules whose folder name contains this string</param>
    /// <param name="titleContains">Optional filter to include only modules whose title contains this string</param>
    /// <returns>Result containing an array of ModuleThin objects representing the filtered modules</returns>
    /// <exception cref="PolarionClientException">Thrown when there is an error communicating with the Polarion service</exception>
    /// <remarks>
    /// Both filters are matched as literal substrings (case-insensitive): quotes are escaped and
    /// the LIKE wildcards <c>%</c> and <c>_</c> have no special meaning.
    /// </remarks>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<ModuleThin[]>> GetModulesThinAsync(string? excludeSpaceNameContains = null, string? titleContains = null)
    {
        try
        {
            var sqlQuery =
            "SELECT doc.C_PK FROM MODULE doc, PROJECT proj " +
            $"WHERE proj.C_ID = '{PolarionSql.EscapeLiteral(_config.ProjectId)}' " +
            "AND doc.FK_URI_PROJECT = proj.C_URI ";

            if (!string.IsNullOrWhiteSpace(excludeSpaceNameContains))
            {
                sqlQuery += $"AND UPPER(doc.C_MODULEFOLDER) NOT LIKE '%{PolarionSql.EscapeLikePattern(excludeSpaceNameContains.ToUpperInvariant())}%' {PolarionSql.LikeEscapeClause} ";
            }

            if (!string.IsNullOrWhiteSpace(titleContains))
            {
                sqlQuery += $"AND UPPER(doc.C_TITLE) LIKE '%{PolarionSql.EscapeLikePattern(titleContains.ToUpperInvariant())}%' {PolarionSql.LikeEscapeClause} ";
            }

            var result = await _trackerClient.queryModulesBySQLAsync(
                new(
                sqlQuery: sqlQuery,
                fields: ["id", "title", "type", "status", "moduleFolder", "moduleLocation"]));


            if (result is null)
            {
                return Result.Fail("Failed to get documents");
            }

            return Result.Ok(ToModuleThins(result.queryModulesBySQLReturn));
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get documents. {ex.Message}");
        }
    }

    /// <summary>
    /// Maps queryModulesBySQL rows to <see cref="ModuleThin"/>, sorted by title. A null array (zero rows),
    /// null rows and rows without an id are skipped; a missing type or status becomes an empty string.
    /// </summary>
    private static ModuleThin[] ToModuleThins(Module[]? rows) =>
        (rows ?? [])
            .Where(x => x?.id != null)
            .Select(x => new ModuleThin(x.id, x.title, x.type?.id ?? string.Empty, x.status?.id ?? string.Empty, x.moduleFolder, x.moduleLocation, x.uri))
            .OrderBy(x => x.Title)
            .ToArray();
}
