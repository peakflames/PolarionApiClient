
namespace PolarionApiClient.Core
{
    public record PolarionClientConfiguration
    {
        public required string ServerUrl { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string ProjectId { get; init; }
        public int TimeoutSeconds { get; init; } = 30;
    }
}
