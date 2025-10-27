
namespace Contracts.Options
{
    /// <summary>
    /// Database settings.
    /// </summary>
    public sealed class DatabaseOptions
    {
        /// <summary>Connection string.</summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
