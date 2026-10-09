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
            $"WHERE proj.C_ID = '{PolarionSql.EscapeLiteral(_config.ProjectId)}' " +
            "AND doc.FK_URI_PROJECT = proj.C_URI " +
            $"AND doc.C_MODULEFOLDER = '{PolarionSql.EscapeLiteral(spaceName ?? string.Empty)}' ";

            var result = await _trackerClient.queryModulesBySQLAsync(
                new(
                sqlQuery: sqlQuery,
                fields: ["id", "title", "type", "status", "moduleFolder", "moduleLocation"]));


            if (result is null)
            {
                return Result.Fail("Failed to get documents");
            }

            // A query that matches nothing comes back with a null array; that is a valid, empty result.
            return Result.Ok(ToModuleThins(result.queryModulesBySQLReturn));
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get documents. {ex.Message}");
        }
    }
}
