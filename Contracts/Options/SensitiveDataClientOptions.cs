namespace Contracts.Options
{
    /// <summary>
    /// Sensitive data access client settings.
    /// </summary>
    public sealed class SensitiveDataClientOptions
    {
        public const string ConfigKey = "SecretsLoader";

        /// <summary>
        /// The time interval between attempts to access the sensitive data for the initial load.
        /// </summary>
        public int InitialReadIntervalMs { get; set; }

        /// <summary>
        /// The time interval between attempts to access the sensitive data for the refresh,
        /// when zero - the data is not refreshed after the initial load.
        /// </summary>
        public int RefreshReadIntervalMs { get; set; }
    }
}
