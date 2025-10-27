
namespace Contracts.Options
{
    /// <summary>
    /// SSE Streaming related settings.
    /// </summary>
    public sealed class MessageStreamOptions
    {
        public const string ConfigKey = "MessageStream";

        /// <summary>
        /// How frequently to send SSE keep-alive comments (measured in miliseconds).
        /// </summary>
        public int KeepAliveIntervalMs { get; set; }

        /// <summary>
        /// How far back to replay messages on connect (measured in seconds).
        /// </summary>
        public int ReplayLookbackS { get; set; }
    }
}
