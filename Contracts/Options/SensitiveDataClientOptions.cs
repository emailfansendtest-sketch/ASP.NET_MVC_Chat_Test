namespace MVC_SSL_Chat.Internal
{
    /// <summary>
    /// Sensitive data access client settings.
    /// </summary>
    public sealed class SensitiveDataClientOptions
    {
        public const string ConfigKey = "SecretsLoader";

        /// <summary>
        /// The time interval between the sensitive data accesses.
        /// </summary>
        public int ConsequentReadDelayMs { get; set; }
    }
}
