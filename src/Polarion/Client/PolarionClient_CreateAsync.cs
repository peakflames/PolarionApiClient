namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public static async Task<Result<PolarionClient>> CreateAsync(PolarionClientConfiguration config)
    {
        // Create binding for Session service
        var binding = new BasicHttpBinding();
        if (config.ServerUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
        }

        binding.MaxReceivedMessageSize = 10_000_000; // 10 MB
        binding.OpenTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.CloseTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.SendTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.ReceiveTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.AllowCookies = true;

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

            // Configure client to use the session ID
            var sessionHeaderValue = sessionHeader.Value;
            var sessionHeaderBehavior = new SessionHeaderBehavior(sessionHeaderValue);
            trackerClient.Endpoint.EndpointBehaviors.Add(sessionHeaderBehavior);

            // Share cookies between clients if available
            if (cookieContainer != null)
            {
                var cookieBehavior = new CookieContainerBehavior(cookieContainer);
                trackerClient.Endpoint.EndpointBehaviors.Add(cookieBehavior);
            }

            return new PolarionClient(trackerClient, config);
        }
        catch (Exception ex) when (ex is not PolarionClientException)
        {
            return Result.Fail<PolarionClient>($"Failed to initialize Polarion client: {ex.Message}");
        }
    }


}
