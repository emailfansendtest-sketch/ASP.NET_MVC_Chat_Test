using System.Collections.Concurrent;

using Application.DTO;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;

namespace Application.Implementations.ChatEvents
{
    internal sealed class ChatEventBus : IChatEventBus
    {
        private readonly IChatEventSubscriberFactory _chatEventSubscriberFactory;

        private readonly ConcurrentDictionary<IMessageStreamWriter, IChatEventSubscriber> _subs = new();

        public ChatEventBus( IChatEventSubscriberFactory chatEventSubscriberFactory )
        {
            _chatEventSubscriberFactory = chatEventSubscriberFactory;
        }

        public void Subscribe( IMessageStreamWriter listener )
        {
            if( _subs.ContainsKey( listener ) )
            {
                return;
            }

            var sub = _chatEventSubscriberFactory.Create( listener );
            _subs.TryAdd( listener, sub );
        }

        public void Unsubscribe( IMessageStreamWriter listener )
        {
            if( _subs.TryRemove( listener, out var sub ) )
            {
                sub.Dispose();
            }
        }

        public Task PublishAsync( ChatMessageDto message )
        {
            foreach( var (listener, sub) in _subs )
            {
                sub.TryWrite( message );
            }
            return Task.CompletedTask;
        }
    }
}
