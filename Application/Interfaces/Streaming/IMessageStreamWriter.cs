using Application.DTO;

namespace Application.Interfaces.Streaming
{
    /// <summary>
    /// Transport used to push <see cref="ChatMessageDto"/> payloads to a connected client.
    /// Implementations typically write to an HTTP response (e.g., SSE).
    /// </summary>
    public interface IMessageStreamWriter
    {
        /// <summary>
        /// Writes a single chat message to the client and flushes the output.
        /// </summary>
        /// <param name="message">Message DTO to be delivered to the client.</param>
        /// <param name="ct">Cancellation token for the write operation.</param>
        Task WriteMessageAsync( ChatMessageDto message, CancellationToken ct = default );

        /// <summary>
        /// Sends a transport-specific keep-alive (e.g., SSE comment) to prevent idle timeouts.
        /// </summary>
        /// <param name="ct">Cancellation token for the write operation.</param>
        Task WriteKeepAliveAsync( CancellationToken ct = default );
    }
}
