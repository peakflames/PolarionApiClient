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
    private readonly TrackerWebService _trackerClient;
    private readonly PolarionClientConfiguration _config;
    
    
    public PolarionClient(TrackerWebService trackerClient, PolarionClientConfiguration config)
    {
        _trackerClient = trackerClient ?? throw new ArgumentNullException(nameof(trackerClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
 
}

