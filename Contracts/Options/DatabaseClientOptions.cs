
namespace Contracts.Options
{
    /// <summary>
    /// Database access client settings.
    /// </summary>
    public class DatabaseClientOptions
    {
        public const string ConfigKey = "DatabaseClient";

        /// <summary>
        /// The maximum amount of database access attempt repeats.
        /// </summary>
        public int MaximumRetryCount { get; set; }

        /// <summary>
        /// The time interval between database access attempt repeats in milliseconds.
        /// </summary>
        public int IntervalBetweenRetriesMs { get; set; }
    }
}
