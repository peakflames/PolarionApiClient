namespace Polarion;

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