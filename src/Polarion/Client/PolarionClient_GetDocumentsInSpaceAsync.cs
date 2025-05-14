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
    /// Get all documents in a space.
    /// </summary>
    /// <param name="spaceName">Name of the space</param>
    /// <returns>Result with the documents</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Module[]>> GetDocumentsInSpaceAsync(string spaceName)
    {
        try
        {
            var result = await _trackerClient.getModuleUrisAsync(new(_config.ProjectId, spaceName));
            if (result is null)
            {
                return Result.Fail<Module[]>("Failed to get documents");
            }

            var uris = result.getModuleUrisReturn;
            var modules = new List<Module>();
            foreach (var uri in uris)
            {
                var moduleResult = await _trackerClient.getModuleByUriAsync(new(uri));
                if (moduleResult is null)
                {
                    return Result.Fail<Module[]>("Failed to get documents");
                }

                var document = moduleResult.getModuleByUriReturn;
                modules.Add(document);
            }

            return Result.Ok(modules.ToArray());
        }
        catch (Exception ex)
        {
            return Result.Fail<Module[]>($"Failed to get documents. {ex.Message}");
        }
    }
}
