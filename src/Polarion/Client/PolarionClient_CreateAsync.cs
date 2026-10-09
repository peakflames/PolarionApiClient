namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public static async Task<Result<PolarionClient>> CreateAsync(PolarionClientConfiguration config)
    {
        // Create binding shared by the Session, Tracker and Project services
        var bindingResult = CreateBinding(config);
        if (bindingResult.IsFailed)
        {
            return Result.Fail<PolarionClient>(bindingResult.Errors);
        }

        var binding = bindingResult.Value;

        // Create session endpoint and client
        var sessionEndpoint = new EndpointAddress($"{config.ServerUrl.TrimEnd('/')}/polarion/ws/services/SessionWebService");
        var sessionClient = new SessionWebServiceClient(binding, sessionEndpoint);

        // Add message inspector to capture session details
        var messageInspector = new MessageInspector();
        var endpointBehavior = new EndpointBehavior(messageInspector);
        sessionClient.Endpoint.EndpointBehaviors.Add(endpointBehavior);

        try
        {
            // Login using the Session service
            var loginResponse = await sessionClient.logInAsync(config.Username, config.Password);
            if (loginResponse is null)
            {
                return Result.Fail<PolarionClient>("Login failed - invalid credentials");
            }

            // Extract session ID from response
            var sessionHeader = messageInspector.LastResponseEnvelope?
                .Descendants(XName.Get("sessionID", "http://ws.polarion.com/session"))
                .FirstOrDefault();

            if (sessionHeader == null)
            {
                return Result.Fail<PolarionClient>("Failed to get session ID from response");
            }

            // Extract cookies from the client
            var cookieContainer = sessionClient.InnerChannel.GetProperty<IHttpCookieContainerManager>()?.CookieContainer;

            // Create tracker client with same binding
            var trackerEndpoint = new EndpointAddress($"{config.ServerUrl.TrimEnd('/')}/polarion/ws/services/TrackerWebService");
            var trackerClient = new TrackerWebServiceClient(binding, trackerEndpoint);

            // Create project client with the same binding, alongside the tracker client
            var projectEndpoint = new EndpointAddress($"{config.ServerUrl.TrimEnd('/')}/polarion/ws/services/ProjectWebService");
            var projectClient = new Polarion.Generated.Project.ProjectWebServiceClient(binding, projectEndpoint);

            // Configure clients to use the session ID
            var sessionHeaderValue = sessionHeader.Value;
            var sessionHeaderBehavior = new SessionHeaderBehavior(sessionHeaderValue);
            trackerClient.Endpoint.EndpointBehaviors.Add(sessionHeaderBehavior);
            projectClient.Endpoint.EndpointBehaviors.Add(new SessionHeaderBehavior(sessionHeaderValue));

            // Share cookies between clients if available
            if (cookieContainer != null)
            {
                var cookieBehavior = new CookieContainerBehavior(cookieContainer);
                trackerClient.Endpoint.EndpointBehaviors.Add(cookieBehavior);
                projectClient.Endpoint.EndpointBehaviors.Add(new CookieContainerBehavior(cookieContainer));
            }

            return new PolarionClient(trackerClient, projectClient, config);
        }
        catch (Exception ex) when (ex is not PolarionClientException)
        {
            return Result.Fail<PolarionClient>($"Failed to initialize Polarion client: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the HTTP binding used by all service clients from the configuration.
    /// </summary>
    /// <param name="config">The client configuration</param>
    /// <returns>The binding, or a failure when <see cref="PolarionClientConfiguration.MaxReceivedMessageSize"/>
    /// is set to zero or a negative value</returns>
    internal static Result<BasicHttpBinding> CreateBinding(PolarionClientConfiguration config)
    {
        if (config.MaxReceivedMessageSize is <= 0)
        {
            return Result.Fail<BasicHttpBinding>(
                $"MaxReceivedMessageSize must be greater than zero when set (was {config.MaxReceivedMessageSize}).");
        }

        var binding = new BasicHttpBinding();
        if (config.ServerUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
        }

        // No cap by default — large Polarion projects exceed 10 MB. Callers may opt into a lower cap.
        binding.MaxReceivedMessageSize = config.MaxReceivedMessageSize ?? int.MaxValue;
        binding.OpenTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.CloseTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.SendTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.ReceiveTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.AllowCookies = true;

        return Result.Ok(binding);
    }


}
