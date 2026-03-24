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
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<ModuleThin[]>> GetModulesThinAsync(string? excludeSpaceNameContains = null, string? titleContains = null)
    {
        try
        {
            var sqlQuery =
            "SELECT doc.C_PK FROM MODULE doc, PROJECT proj " +
            $"WHERE proj.C_ID = '{EscapeSqlLiteral(_config.ProjectId)}' " +
            "AND doc.FK_PROJECT = proj.C_PK ";

            if (!string.IsNullOrWhiteSpace(excludeSpaceNameContains))
            {
                sqlQuery += $"AND UPPER(doc.C_MODULEFOLDER) NOT LIKE '%{EscapeSqlLiteral(excludeSpaceNameContains.ToUpperInvariant())}%' ";
            }

            if (!string.IsNullOrWhiteSpace(titleContains))
            {
                sqlQuery += $"AND UPPER(doc.C_TITLE) LIKE '%{EscapeSqlLiteral(titleContains.ToUpperInvariant())}%' ";
            }

            var result = await _trackerClient.queryModulesBySQLAsync(
                new(
                sqlQuery: sqlQuery,
                fields: ["id", "moduleName", "title", "type", "status", "moduleFolder", "moduleLocation"]));


            if (result is null)
            {
                return Result.Fail("Failed to get documents");
            }

            if (result?.queryModulesBySQLReturn is null || result.queryModulesBySQLReturn.Length == 0)
            {
                return Result.Ok(Array.Empty<ModuleThin>());
            }

            var modules = result.queryModulesBySQLReturn
                .Where(x => x != null)
                .Select(x =>
                {
                    var module = x!;

                    var moduleId = !string.IsNullOrWhiteSpace(module.id)
                        ? module.id
                        : module.moduleName;

                    if (string.IsNullOrWhiteSpace(moduleId))
                    {
                        return null;
                    }

                    return new ModuleThin(
                        moduleId,
                        module.title ?? string.Empty,
                        module.type?.id ?? string.Empty,
                        module.status?.id ?? string.Empty,
                        module.moduleFolder ?? string.Empty,
                        module.moduleLocation ?? string.Empty,
                        module.uri ?? string.Empty);
                })
                    .OfType<ModuleThin>();

            // sort the list of documents by title
            modules = modules.OrderBy(x => x.Title).ToList();

            return Result.Ok(modules.ToArray());
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get documents. {ex.Message}");
        }
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }
}
