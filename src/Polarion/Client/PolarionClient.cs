namespace Polarion;

public partial class PolarionClient(
    TrackerWebService trackerClient, Polarion.Generated.Project.ProjectWebService projectClient, PolarionClientConfiguration config) : IPolarionClient
{
    private readonly TrackerWebService _trackerClient = trackerClient ?? throw new ArgumentNullException(nameof(trackerClient));
    private readonly Polarion.Generated.Project.ProjectWebService _projectClient = projectClient ?? throw new ArgumentNullException(nameof(projectClient));
    private readonly PolarionClientConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ReverseMarkdown.Converter _markdownConverter = new ReverseMarkdown.Converter();
    public TrackerWebService TrackerService => _trackerClient;
    public Polarion.Generated.Project.ProjectWebService ProjectService => _projectClient;
}

