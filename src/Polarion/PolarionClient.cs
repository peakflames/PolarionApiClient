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


    public PolarionClient(TrackerWebService trackerClient, PolarionClientConfiguration config)
    {
        _trackerClient = trackerClient ?? throw new ArgumentNullException(nameof(trackerClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem>> GetWorkItemByIdAsync(string workItemId)
    {
        try
        {
            var result = await _trackerClient.getWorkItemByIdAsync(new(_config.ProjectId, workItemId));
            return result is null ?  Result.Fail<WorkItem>("Work item not found") : result.getWorkItemByIdReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException($"Failed to get work item {workItemId}", ex);
        }
    }


    /// <summary>
    /// Query for available workitems. This will only query for the items.
    /// If you also want the Workitems to be retrieved, used searchWorkitemFullItem.
    ///
    /// For retrieving custom field using field_list, use the following syntax:
    /// field_list=['customFields.SomeField']
    /// </summary>
    /// <param name="query">The query to use while searching</param>
    /// <param name="order">Order by</param>
    /// <param name="field_list">list of fields to retrieve for each search result</param>
    /// <returns>Search results but only with the given fields set</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemAsync(string query, string order = "Created", List<string>? field_list = null)
    {
        try
        {
            if (field_list is null)
            {
                field_list = ["id"];
            }

            query += $" AND project.id:{_config.ProjectId}";

            var result = await _trackerClient.queryWorkItemsAsync(new(query, order, field_list.ToArray()));
            return result.queryWorkItemsReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }

    /// <summary>
    /// Query for available workitems in a baseline. This will only query for the items.
    /// If you also want the Workitems to be retrieved, used searchWorkitemFullItemInBaseline.
    ///
    /// For retrieving custom field using field_list, use the following syntax:
    /// field_list=['customFields.FieldName']
    /// </summary>
    /// <param name="baselineRevision">The revision number of the baseline to search in</param>
    /// <param name="query">The query to use while searching</param>
    /// <param name="order">Order by</param>
    /// <param name="field_list">list of fields to retrieve for each search result</param>
    /// <returns>Search results but only with the given fields set</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<WorkItem[]>> SearchWorkitemInBaselineAsync(string baselineRevision, string query, string order = "Created", List<string>? field_list = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baselineRevision))
            {
                return Result.Fail<WorkItem[]>("Baseline revision cannot be null or empty");
            }

            if (field_list is null)
            {
                field_list = ["id"];
            }

            query += $" AND project.id:{_config.ProjectId}";

            var result = await _trackerClient.queryWorkItemsInRevisionAsync(new(query, order, baselineRevision,field_list.ToArray()));
            return result.queryWorkItemsInRevisionReturn is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsInRevisionReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail<WorkItem[]>($"Failed to execute work item query. {ex.Message}");
        }
    }


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


// Helper classes for .NET 8.0
public class MessageInspector : IClientMessageInspector
{
    public XDocument? LastResponseEnvelope { get; private set; }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel) => null;

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var buffer = reply.CreateBufferedCopy(int.MaxValue);
        reply = buffer.CreateMessage();
        var messageCopy = buffer.CreateMessage();

        LastResponseEnvelope = XDocument.Parse(messageCopy.ToString());
    }
}

public class EndpointBehavior : IEndpointBehavior
{
    private readonly IClientMessageInspector _inspector;

    public EndpointBehavior(IClientMessageInspector inspector)
    {
        _inspector = inspector;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(_inspector);
    }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}

public class SessionHeaderBehavior : IEndpointBehavior
{
    private readonly string _sessionId;

    public SessionHeaderBehavior(string sessionId)
    {
        _sessionId = sessionId;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(new SessionHeaderInspector(_sessionId));
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}

public class SessionHeaderInspector : IClientMessageInspector
{
    private readonly string _sessionId;

    public SessionHeaderInspector(string sessionId)
    {
        _sessionId = sessionId;
    }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var header = MessageHeader.CreateHeader(
            "sessionID",
            "http://ws.polarion.com/session",
            _sessionId);

        request.Headers.Add(header);
        return null;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState) { }
}

public class CookieContainerBehavior : IEndpointBehavior
{
    private readonly CookieContainer _cookieContainer;

    public CookieContainerBehavior(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
        bindingParameters.Add(_cookieContainer);
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}
