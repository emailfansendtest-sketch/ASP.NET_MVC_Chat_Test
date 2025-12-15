namespace Application.Interfaces.Streaming
{
    /// <summary>
    /// Drives a server-to-client message stream for one connected client.
    /// </summary>
    public interface IMessageStreamService
    {
        /// <summary>
        /// Streams messages to a single client using the provided low-level writer.
        /// The call should not return until the client disconnects or <paramref name="ct"/> is canceled.
        /// </summary>
        /// <param name="writer">
        /// Transport writer for the current client (e.g., SSE response writer).
        /// Implementations should not cache this writer beyond the lifetime of the call.
        /// </param>
        /// <param name="ct">Cancellation token that signals client disconnect or server shutdown.</param>
        Task StreamForClientAsync( IMessageStreamWriter writer, CancellationToken ct );
    }
}
