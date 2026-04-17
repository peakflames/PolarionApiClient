namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get all modules in a space.
    /// </summary>
    /// <param name="spaceName">Name of the space</param>
    /// <returns>Result with the documents</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<ModuleThin[]>> GetModulesInSpaceThinAsync(string spaceName)
    {
        try
        {
            var sqlQuery =
            "SELECT doc.C_PK FROM MODULE doc, PROJECT proj " +
            $"WHERE proj.C_ID = '{EscapeSqlLiteral(_config.ProjectId)}' " +
            "AND doc.FK_PROJECT = proj.C_PK " +
            $"AND doc.C_MODULEFOLDER = '{EscapeSqlLiteral(spaceName)}' ";

            var result = await _trackerClient.queryModulesBySQLAsync(
                new(
                sqlQuery: sqlQuery,
                fields: ["id", "moduleName", "title", "type", "status", "moduleFolder", "moduleLocation"]));


            if (result is null)
            {
                return Result.Fail("Failed to get documents");
            }

            if (result.queryModulesBySQLReturn is null || result.queryModulesBySQLReturn.Length == 0)
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
}
