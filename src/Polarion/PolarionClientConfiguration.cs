using System.Text.Json.Serialization;

namespace Polarion
{
    public record PolarionClientConfiguration
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? Mechanism { get; set; }
        public string? Token { get; set; }
        public string ProjectId { get; set; }
        public int TimeoutSeconds { get; set; } = 30;

        [JsonConstructor]
        public PolarionClientConfiguration(string serverUrl, string username, string projectId, int timeoutSeconds = 30, string? password = null, string? mechanism = null, string? token = null)
        {
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
            Mechanism = mechanism;
            Token = token;
            ProjectId = projectId;
            TimeoutSeconds = timeoutSeconds;
        }
    }
}
