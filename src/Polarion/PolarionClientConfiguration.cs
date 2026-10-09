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

        /// <summary>
        /// Optional cap, in bytes, on the size of a single SOAP response.
        /// </summary>
        /// <remarks>
        /// Null (the default) means no cap: the transport accepts responses up to <see cref="int.MaxValue"/>
        /// bytes, which large projects need. Set a lower value to bound the memory a single call can use;
        /// a response larger than the cap fails with a WCF quota-exceeded error. When set, the value must
        /// be greater than zero, otherwise <see cref="PolarionClient.CreateAsync"/> returns a failure.
        /// </remarks>
        public int? MaxReceivedMessageSize { get; set; }
    }
}
