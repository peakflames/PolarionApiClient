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
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<List<string>>> GetDocumentSpacesAsync()
    {
        try
        {
            var result = await _trackerClient.getDocumentSpacesAsync(new(_config.ProjectId));
            if (result is null)
            {
                return Result.Fail<List<string>>("Failed to get spaces");
            }

            var names = result.getDocumentSpacesReturn;
            // sort the list of spaces
            var spaces = names.Select(x => x.ToString()).OrderBy(x => x).ToList();
            return Result.Ok(spaces);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<string>>($"Failed to get spaces. {ex.Message}");
        }
    }
}
