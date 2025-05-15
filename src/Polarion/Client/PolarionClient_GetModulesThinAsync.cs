using System.Net;
using System.Reflection.Metadata;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml.Linq;


namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets modules in the project that match the specified criteria
    /// </summary>
    /// <param name="excludeFolderNameContains">Optional filter to exclude modules whose folder name contains this string</param>
    /// <param name="titleContains">Optional filter to include only modules whose title contains this string</param>
    /// <returns>Result containing an array of ModuleThin objects representing the filtered modules</returns>
    /// <exception cref="PolarionClientException">Thrown when there is an error communicating with the Polarion service</exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<ModuleThin[]>> GetModulesThinAsync(string? excludeFolderNameContains = null, string? titleContains = null)
    {
        try
        {
            var sqlQuery =
            "SELECT doc.C_PK FROM MODULE doc, PROJECT proj " +
            $"WHERE proj.C_ID = '{_config.ProjectId}' " +
            "AND doc.FK_URI_PROJECT = proj.C_URI ";

            if (excludeFolderNameContains is not null)
            {
                sqlQuery += $"AND doc.C_MODULEFOLDER NOT LIKE '%{excludeFolderNameContains}%' ";
            }

            if (titleContains is not null)
            {
                sqlQuery += $"AND doc.C_TITLE LIKE '%{titleContains}%' ";
            }

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
