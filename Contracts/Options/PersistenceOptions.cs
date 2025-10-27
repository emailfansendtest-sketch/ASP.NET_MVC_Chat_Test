
namespace Contracts.Options
{
    /// <summary>
    /// Database usage settings.
    /// </summary>
    public sealed class PersistenceOptions
    {
        public const string ConfigKey = "Persistence";

        /// <summary>How often to flush in-memory messages to the DB (in miliseconds).</summary>
        public int FlushIntervalMs { get; set; }
    }
}
