using Application.DTO;

namespace Application.Interfaces.ChatEvents
{
    /// <summary>
    /// Represents a per-listener subscription that buffers <see cref="ChatMessageDto"/> items
    /// and forwards them to the bound transport writer.
    /// Intended to make publishing non-blocking (client I/O happens on the subscriber side).
    /// </summary>
    internal interface IChatEventSubscriber : IDisposable
    {
        /// <summary>
        /// Enqueues a message for best-effort delivery to the listener.
        /// Implementations may drop messages when overloaded (bounded buffering).
        /// </summary>
        /// <param name="message">Message DTO to enqueue.</param>
        void TryWrite( ChatMessageDto message );
    }
}
