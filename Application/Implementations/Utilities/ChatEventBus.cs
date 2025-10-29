using Application.Interfaces.Streaming;
using Application.Interfaces.Utilities;
using DomainModels;

using Microsoft.Extensions.Logging;

namespace Application.Implementations.Utilities
{
    public class ChatEventBus : IChatEventBus
    {
        private readonly ILogger<ChatEventBus> _logger;
        private readonly List<IMessageStreamWriter> _listeners = new();

        public ChatEventBus( ILogger<ChatEventBus> logger )
        {
            _logger = logger;
        }

        public async Task PublishAsync( ChatMessage message )
        {
            // fire-and-forget per listener to avoid blocking publisher
            foreach(var listener in _listeners.ToArray())
            {
                await NotifySafeAsync( listener, message );
            }
        }

        public void Subscribe( IMessageStreamWriter listener )
        {
            _listeners.Add( listener );
            _logger.LogDebug( "Listener subscribed: {Listener}", listener.GetType().Name );
        }

        public void Unsubscribe( IMessageStreamWriter listener )
        {
            _listeners.Remove( listener );
            _logger.LogDebug( "Listener unsubscribed: {Listener}", listener.GetType().Name );
        }

        private async Task NotifySafeAsync( IMessageStreamWriter listener, ChatMessage message )
        {
            try
            {
                await listener.WriteMessageAsync( message );
            }
            catch(Exception ex)
            {
                _logger.LogError( ex, "Listener {Listener} failed to process message", listener.GetType().Name );
            }
        }
    }
}
