using Application.Interfaces.Streaming;

namespace Application.Interfaces.ChatEvents
{
    /// <summary>
    /// Factory for creating per-connection chat event subscribers.
    /// </summary>
    internal interface IChatEventSubscriberFactory
    {
        /// <summary>
        /// Creates a subscriber that is bound to the given listener.
        /// </summary>
        /// <param name="listener">Listener that will receive streamed messages.</param>
        IChatEventSubscriber Create( IMessageStreamWriter listener );
    }
}
