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
    public async Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId)
    {
        try
        {
            var result = await _trackerClient.getWorkItemByIdAsync(new(_config.ProjectId, workItemId));
            return result is null ? Result.Fail<WorkItem>("Work item not found") : result.getWorkItemByIdReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get work item {workItemId}", ex);
        }
    }
}
