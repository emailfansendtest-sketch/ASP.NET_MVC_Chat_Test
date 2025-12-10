using Application.Interfaces.Streaming;

namespace Application.Interfaces.ChatEvents
{
    internal interface IChatEventSubscriberFactory
    {
        IChatEventSubscriber Create( IMessageStreamWriter listener );
    }
}
