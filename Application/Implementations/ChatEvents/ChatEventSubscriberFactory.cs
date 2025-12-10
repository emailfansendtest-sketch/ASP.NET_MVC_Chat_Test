using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;
using Contracts.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Implementations.ChatEvents
{
    internal class ChatEventSubscriberFactory : IChatEventSubscriberFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ChatEventOptions _options;

        public ChatEventSubscriberFactory( 
            ILoggerFactory loggerFactory, 
            IOptions<ChatEventOptions> options )
        {
            _loggerFactory = loggerFactory;
            _options = options.Value;
        }

        public IChatEventSubscriber Create( IMessageStreamWriter listener )
            => new ChatEventSubscriber( _options, listener, _loggerFactory );
    }
}
