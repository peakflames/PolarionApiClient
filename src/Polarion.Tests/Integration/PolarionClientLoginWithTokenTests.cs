using Xunit;
using Polarion.Tests.Helpers;
using FluentAssertions;

namespace Polarion.Tests.Integration;

public class PolarionClientLoginWithTokenTests : IAsyncLifetime
{
    private readonly TestConfiguration _config;
    private PolarionClient _client = null!;

    public PolarionClientLoginWithTokenTests()
    {
        // Load configuration from the test settings file
        _config = TestConfigurationLoader.Load("../../../appsettings.loginWithToken.test.json");
    }

    public async Task InitializeAsync()
    {
        // Create the client before each test
        var result = await PolarionClient.CreateAsync(_config.PolarionClient);
        result.IsSuccess.Should().BeTrue("Polarion client should be created successfully");
        _client = result.Value;
        _client.Should().NotBeNull("Polarion client should be created successfully");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }


    [Fact]
    public async Task GetWorkItemById_ShouldReturnValidWorkItem()
    {
        // Arrange
        var workItemId = _config.TestScenarioData.GetWorkItemByIdAsyncWorkItemId;

        // Act
        var result = await _client.GetWorkItemByIdAsync(workItemId);

        // Assert
        result.IsSuccess.Should().BeTrue("Work item retrieval should succeed");
        var workItem = result.Value;
        workItem.Should().NotBeNull();
        workItem.id.Should().NotBeNullOrEmpty();
        workItem.title.Should().NotBeNullOrEmpty();
        workItem.type.id.Should().NotBeNullOrEmpty();
        workItem.status.id.Should().NotBeNullOrEmpty();
        workItem.author.name.Should().NotBeNullOrEmpty();
        workItem.description.content.Should().NotBeNullOrEmpty();
    }
}
