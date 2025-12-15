using Application.DTO;
using Application.Interfaces.Streaming;

namespace Application.Interfaces.ChatEvents
{
    /// <summary>
    /// In-process pub/sub bus for broadcasting <see cref="ChatMessageDto"/> events to live connections.
    /// Publication is best-effort; delivery/backpressure is handled per-subscriber.
    /// Implementations should be thread-safe.
    /// </summary>
    public interface IChatEventBus
    {
        /// <summary>
        /// Publishes a message DTO to all current subscribers.
        /// This call should be non-blocking with respect to client I/O.
        /// </summary>
        /// <param name="messageDto">The message DTO to broadcast.</param>
        Task PublishAsync( ChatMessageDto messageDto );

        /// <summary>
        /// Subscribes a streaming listener to receive future messages.
        /// The bus should not hold the listener beyond the lifetime of the connection.
        /// </summary>
        /// <param name="listener">Listener to notify on new messages.</param>
        void Subscribe( IMessageStreamWriter listener );

        /// <summary>
        /// Unsubscribes a previously subscribed listener. Safe to call multiple times.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        void Unsubscribe( IMessageStreamWriter listener );
    }
}
