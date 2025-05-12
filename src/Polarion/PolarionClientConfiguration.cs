using System.Text.Json.Serialization;

namespace Polarion
{
    public record PolarionClientConfiguration
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProjectId { get; set; }
        public int TimeoutSeconds { get; set; } = 30;

        [JsonConstructor]
        public PolarionClientConfiguration(string serverUrl, string username, string password, string projectId, int timeoutSeconds = 30)
        {
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
            ProjectId = projectId;
            TimeoutSeconds = timeoutSeconds;
        }
    }
}
