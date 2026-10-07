using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml.Linq;
using FluentAssertions;
using Polarion;
using Polarion.Generated.Project;
using Polarion.Generated.Tracker;
using Xunit;
using Xunit.Abstractions;

namespace Polarion.Tests.Unit;

/// <summary>
/// Serializes requests through the real WCF client pipeline and inspects the SOAP body that
/// would go on the wire. The request is captured by a client message inspector and aborted
/// before any transport is used, so no server connection is made.
/// </summary>
/// <remarks>
/// The DispatchProxy fakes used elsewhere receive request objects, not XML, so they cannot see
/// how a null argument is serialized. Polarion rejects an omitted optional URI element
/// ("URI scheme null is not subterra") but accepts an explicit xsi:nil element.
/// </remarks>
public class SoapRequestSerializationTests(ITestOutputHelper output)
{
    private const string TrackerNs = "http://ws.polarion.com/TrackerWebService-impl";
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private const string ModuleUri = "subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}Space#Doc";

    [Fact]
    public async Task GetModuleWorkItemsAsync_NullParent_SendsNilParentElement()
    {
        var body = await CaptureBodyAsync(async tracker =>
        {
            var project = DispatchProxy.Create<ProjectWebService, Fakes.FakeService>();
            var config = new PolarionClientConfiguration("http://localhost/polarion", "user", "not-used", "TestProject");
            await new PolarionClient(tracker, project, config).GetModuleWorkItemsAsync(ModuleUri);
        });

        output.WriteLine(body.ToString());
        body.Name.Should().Be(XName.Get("getModuleWorkItems", TrackerNs));
        body.Elements().Select(e => e.Name.LocalName).Should().StartWith(["moduleURI", "parentWorkItemURI", "deep"]);
        var parent = body.Element(XName.Get("parentWorkItemURI", TrackerNs));
        parent.Should().NotBeNull("an omitted parent URI is parsed by Polarion as a URI with a null scheme");
        parent!.Attribute(Xsi + "nil")?.Value.Should().Be("true");
        parent.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetModuleWorkItemsRequest_NullParent_MatchesGetModuleWorkItemUrisRequestForm()
    {
        var items = await CaptureBodyAsync(t => t.getModuleWorkItemsAsync(new getModuleWorkItemsRequest(ModuleUri, null!, true, ["id"])));
        var uris = await CaptureBodyAsync(t => t.getModuleWorkItemUrisAsync(new getModuleWorkItemUrisRequest(ModuleUri, null!, true)));

        output.WriteLine(items.ToString());
        output.WriteLine(uris.ToString());
        var itemsParent = items.Element(XName.Get("parentWorkItemURI", TrackerNs));
        var urisParent = uris.Element(XName.Get("parentWorkItemURI", TrackerNs));

        urisParent.Should().NotBeNull("the getModuleWorkItemUris form is the one Polarion accepts");
        itemsParent.Should().NotBeNull();
        Describe(itemsParent!).Should().Be(Describe(urisParent!));
    }

    [Fact]
    public async Task GetModuleWorkItemsRequest_WithParent_SendsParentValue()
    {
        const string parent = "subterra:data-service:objects:/default/TestProject${WorkItem}WI-1";

        var body = await CaptureBodyAsync(t => t.getModuleWorkItemsAsync(new getModuleWorkItemsRequest(ModuleUri, parent, false, ["id"])));

        var element = body.Element(XName.Get("parentWorkItemURI", TrackerNs))!;
        element.Value.Should().Be(parent);
        element.Attribute(Xsi + "nil").Should().BeNull();
    }

    private static string Describe(XElement element) =>
        $"{element.Name}|nil={element.Attribute(Xsi + "nil")?.Value}|value={element.Value}";

    /// <summary>
    /// Runs <paramref name="call"/> against a real <see cref="TrackerWebServiceClient"/> and returns
    /// the SOAP body's operation element. The message is captured and the call aborted before sending.
    /// </summary>
    internal static async Task<XElement> CaptureBodyAsync(Func<TrackerWebService, Task> call)
    {
        var capture = new CapturingInspector();
        var client = new TrackerWebServiceClient(
            new BasicHttpBinding(),
            new EndpointAddress("http://localhost/polarion/ws/services/TrackerWebService"));
        client.Endpoint.EndpointBehaviors.Add(new CaptureBehavior(capture));

        try
        {
            await call(client);
        }
        catch (RequestCapturedException)
        {
            // Expected: the inspector stops the call once the message is serialized.
        }
        finally
        {
            client.Abort();
        }

        capture.Envelope.Should().NotBeNull("the request should have been serialized");
        var envelope = XDocument.Parse(capture.Envelope!);
        var soapBody = envelope.Root!.Elements().Single(e => e.Name.LocalName == "Body");
        return soapBody.Elements().Single();
    }

    private sealed class RequestCapturedException() : Exception("request captured");

    private sealed class CapturingInspector : IClientMessageInspector
    {
        public string? Envelope { get; private set; }

        public object? BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var buffer = request.CreateBufferedCopy(int.MaxValue);
            request = buffer.CreateMessage();
            Envelope = buffer.CreateMessage().ToString();
            throw new RequestCapturedException();
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
    }

    private sealed class CaptureBehavior(IClientMessageInspector inspector) : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) => clientRuntime.ClientMessageInspectors.Add(inspector);
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        public void Validate(ServiceEndpoint endpoint) { }
    }
}
