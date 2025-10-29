using DomainModels;
using Application.Interfaces.Streaming;

namespace Application.Interfaces.Utilities
{
    /// <summary>
    /// In-memory pub/sub bus for chat events within the application process.
    /// Producers publish messages; streaming writers subscribe to receive real-time updates.
    /// Implementations should be thread-safe.
    /// </summary>
    public interface IChatEventBus
    {
        /// <summary>
        /// Publishes a message to all current subscribers.
        /// </summary>
        /// <param name="message">Message to broadcast.</param>
        Task PublishAsync( ChatMessage message );

        /// <summary>
        /// Subscribes a streaming listener to receive future messages.
        /// The bus should not hold the listener beyond the lifetime of the connection.
        /// </summary>
        /// <param name="listener">Listener to notify on new messages.</param>
        void Subscribe( IMessageStreamWriter listener );

        /// <summary>
        /// Unsubscribes a previously subscribed listener.
        /// Safe to call multiple times.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        void Unsubscribe( IMessageStreamWriter listener );
    }
}
