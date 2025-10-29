using DomainModels;

namespace Application.Interfaces.Streaming
{
    /// <summary>
    /// The transport used to push messages to a connected client.
    /// </summary>
    public interface IMessageStreamWriter
    {
        /// <summary>
        /// Writes a single chat message to the client and flushes the output.
        /// </summary>
        /// <param name="message">Message DTO to be delivered to the client.</param>
        /// <param name="ct">Cancellation token for the write operation.</param>
        Task WriteMessageAsync( ChatMessage message, CancellationToken ct = default );

        /// <summary>
        /// Sends a transport-specific keep-alive (e.g., SSE comment) to prevent idle timeouts.
        /// </summary>
        /// <param name="ct">Cancellation token for the write operation.</param>
        Task WriteKeepAliveAsync( CancellationToken ct = default );
    }
}
