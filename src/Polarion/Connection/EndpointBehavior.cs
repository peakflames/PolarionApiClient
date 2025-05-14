
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