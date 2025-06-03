namespace Polarion;

public partial class PolarionClient(TrackerWebService trackerClient, PolarionClientConfiguration config) : IPolarionClient
{
    private readonly TrackerWebService _trackerClient = trackerClient ?? throw new ArgumentNullException(nameof(trackerClient));
    private readonly PolarionClientConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ReverseMarkdown.Converter _markdownConverter = new ReverseMarkdown.Converter();
    public TrackerWebService TrackerService => _trackerClient;
}

