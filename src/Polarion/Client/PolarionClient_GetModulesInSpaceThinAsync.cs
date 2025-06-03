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
            $"WHERE proj.C_ID = '{_config.ProjectId}' " +
            "AND doc.FK_URI_PROJECT = proj.C_URI " +
            $"AND doc.C_MODULEFOLDER = '{spaceName}' ";

            var result = await _trackerClient.queryModulesBySQLAsync(
                new(
                sqlQuery: sqlQuery,
                fields: ["id", "title", "type", "status", "moduleFolder", "moduleLocation"]));


            if (result is null)
            {
                return Result.Fail("Failed to get documents");
            }

            // only keep the modules whose id is not null
            var modules = result.queryModulesBySQLReturn.Where(x => x.id != null)
                                                        .Select(x => new ModuleThin(x.id, x.title, x.type.id, x.status.id, x.moduleFolder, x.moduleLocation));

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
