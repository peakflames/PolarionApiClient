using System.Reflection;
using Polarion;
using Polarion.Generated.Project;
using Polarion.Generated.Tracker;

namespace Polarion.Tests.Unit.Fakes;

/// <summary>
/// Interface fake built on <see cref="DispatchProxy"/>. Each SOAP method is answered by a handler
/// registered by name; requests are recorded so tests can assert on what the client sent.
/// Calling a method without a handler fails the test with <see cref="NotImplementedException"/>.
/// </summary>
public class FakeService : DispatchProxy
{
    private readonly Dictionary<string, Func<object?, object?>> _handlers = new(StringComparer.Ordinal);

    /// <summary>Requests received, in call order, as (method name, request object).</summary>
    public List<(string Method, object? Request)> Calls { get; } = [];

    /// <summary>
    /// Registers a handler for an async SOAP method (e.g. "queryWorkItemsAsync").
    /// The handler may return the response or throw to simulate a SOAP fault.
    /// </summary>
    public FakeService On<TRequest, TResponse>(string method, Func<TRequest, TResponse> handler)
    {
        _handlers[method] = req => handler((TRequest)req!);
        return this;
    }

    /// <summary>Requests received for the given method.</summary>
    public IEnumerable<TRequest> RequestsFor<TRequest>(string method) =>
        Calls.Where(c => c.Method == method).Select(c => (TRequest)c.Request!);

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        var request = args is { Length: > 0 } ? args[0] : null;
        Calls.Add((targetMethod.Name, request));

        var returnType = targetMethod.ReturnType;
        var resultType = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)
            ? returnType.GetGenericArguments()[0]
            : null;

        if (!_handlers.TryGetValue(targetMethod.Name, out var handler))
        {
            throw new NotImplementedException($"No fake handler registered for {targetMethod.Name}");
        }

        if (resultType is null)
        {
            return handler(request);
        }

        try
        {
            var response = handler(request);
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [response]);
        }
        catch (Exception ex)
        {
            return typeof(Task).GetMethods()
                .Single(m => m.Name == nameof(Task.FromException) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType)
                .Invoke(null, [ex]);
        }
    }
}

/// <summary>
/// Builds a <see cref="PolarionClient"/> wired to fake tracker/project services.
/// </summary>
public static class FakeClient
{
    public const string ProjectId = "TestProject";

    public static (PolarionClient Client, FakeService Tracker) Create(string projectId = ProjectId)
    {
        var tracker = DispatchProxy.Create<TrackerWebService, FakeService>();
        var project = DispatchProxy.Create<ProjectWebService, FakeService>();
        var config = new PolarionClientConfiguration("http://localhost/polarion", "user", "not-used", projectId);
        return (new PolarionClient(tracker, project, config), (FakeService)(object)tracker);
    }
}
