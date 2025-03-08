using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolarionApiClient.Core.Generated;
using PolarionApiClient.Core.Models;

namespace PolarionApiClient.Core;

public class PolarionClient : IPolarionClient
{
    private readonly TrackerWebService _trackerClient;
    private readonly PolarionClientConfiguration _config;
    

    public static PolarionClient Create(PolarionClientConfiguration config)
    {
        var binding = new BasicHttpBinding();

        // Configure the binding for HTTPS if the URL is using HTTPS
        if (config.ServerUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            binding.Security.Mode = BasicHttpSecurityMode.Transport; // Set to Transport for HTTPS
        }
        else
        {
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
        }

        binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
        binding.MaxReceivedMessageSize = 10_000_000; // 10 MB
        binding.OpenTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.CloseTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.SendTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        binding.ReceiveTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds);

        var endpoint = new EndpointAddress($"{config.ServerUrl.TrimEnd('/')}/polarion/ws/services/TrackerWebService");
        var trackerClient = new TrackerWebServiceClient(binding, endpoint);
        trackerClient.ClientCredentials.UserName.UserName = config.Username.Trim();
        trackerClient.ClientCredentials.UserName.Password = config.Password.Trim();

        return new PolarionClient(trackerClient, config);
    }

    public PolarionClient(TrackerWebService trackerClient, PolarionClientConfiguration config)
    {
        _trackerClient = trackerClient ?? throw new ArgumentNullException(nameof(trackerClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

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

    public async Task<Result<WorkItem[]>> QueryWorkItemsAsync(string query, string sort, string[] fields)
    {
        try
        {
            Console.WriteLine($"Executing query: {query}");
            var result = await _trackerClient.queryWorkItemsAsync(new(query, sort, fields));
            return result is null ? Result.Fail<WorkItem[]>("Query failed") : result.queryWorkItemsReturn;
        }
        catch (Exception ex)
        {
            throw new PolarionClientException("Failed to execute work item query", ex);
        }
    }

    
}
