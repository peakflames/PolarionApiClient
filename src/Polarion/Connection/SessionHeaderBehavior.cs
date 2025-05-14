namespace Polarion;

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