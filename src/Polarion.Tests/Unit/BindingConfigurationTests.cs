using System.ServiceModel;
using System.Text.Json;
using FluentAssertions;
using Polarion;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for the transport binding built from <see cref="PolarionClientConfiguration"/> -
/// no server connection required.
/// </summary>
public class BindingConfigurationTests
{
    private static PolarionClientConfiguration Config(string serverUrl = "http://localhost/polarion") =>
        new(serverUrl, "user", "not-used", "TestProject");

    [Fact]
    public void Configuration_MaxReceivedMessageSize_DefaultsToNull()
    {
        Config().MaxReceivedMessageSize.Should().BeNull();
    }

    [Fact]
    public void CreateBinding_Default_HasNoCap()
    {
        var result = PolarionClient.CreateBinding(Config());

        result.IsSuccess.Should().BeTrue();
        result.Value.MaxReceivedMessageSize.Should().Be(int.MaxValue);
        result.Value.MaxBufferSize.Should().Be(int.MaxValue);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65536)]
    [InlineData(10 * 1024 * 1024)]
    [InlineData(int.MaxValue)]
    public void CreateBinding_OptInCap_IsApplied(int cap)
    {
        var config = Config() with { MaxReceivedMessageSize = cap };

        var result = PolarionClient.CreateBinding(config);

        result.IsSuccess.Should().BeTrue();
        result.Value.MaxReceivedMessageSize.Should().Be(cap);
        result.Value.MaxBufferSize.Should().Be(cap, "buffered transfer needs the buffer to follow the cap");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void CreateBinding_NonPositiveCap_Fails(int cap)
    {
        var config = Config() with { MaxReceivedMessageSize = cap };

        var result = PolarionClient.CreateBinding(config);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("MaxReceivedMessageSize");
    }

    [Fact]
    public async Task CreateAsync_NonPositiveCap_FailsBeforeConnecting()
    {
        // The cap is validated before any endpoint is created, so this never dials the server URL.
        var config = Config("http://polarion.example.invalid") with { MaxReceivedMessageSize = 0 };

        var result = await PolarionClient.CreateAsync(config);

        result.IsFailed.Should().BeTrue();
        result.Errors.Single().Message.Should().Contain("MaxReceivedMessageSize");
    }

    [Fact]
    public void CreateBinding_KeepsTimeoutsCookiesAndSecurityMode()
    {
        var config = new PolarionClientConfiguration("https://polarion.example.invalid", "user", "not-used", "TestProject", 45)
        {
            MaxReceivedMessageSize = 1024,
        };

        var binding = PolarionClient.CreateBinding(config).Value;

        binding.Security.Mode.Should().Be(BasicHttpSecurityMode.Transport);
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(45));
        binding.CloseTimeout.Should().Be(TimeSpan.FromSeconds(45));
        binding.SendTimeout.Should().Be(TimeSpan.FromSeconds(45));
        binding.ReceiveTimeout.Should().Be(TimeSpan.FromSeconds(45));
        binding.AllowCookies.Should().BeTrue();
    }

    [Fact]
    public void CreateBinding_HttpUrl_UsesNoTransportSecurity()
    {
        var binding = PolarionClient.CreateBinding(Config()).Value;

        binding.Security.Mode.Should().Be(BasicHttpSecurityMode.None);
    }

    [Fact]
    public void Configuration_FromJson_ReadsOptionalCap()
    {
        const string withCap = """{ "ServerUrl": "http://localhost/polarion", "ProjectId": "P", "MaxReceivedMessageSize": 1048576 }""";
        const string withoutCap = """{ "ServerUrl": "http://localhost/polarion", "ProjectId": "P" }""";

        JsonSerializer.Deserialize<PolarionClientConfiguration>(withCap)!.MaxReceivedMessageSize.Should().Be(1048576);
        JsonSerializer.Deserialize<PolarionClientConfiguration>(withoutCap)!.MaxReceivedMessageSize.Should().BeNull();
    }
}
