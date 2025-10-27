using Application.Interfaces.Streaming;

namespace MVC_SSL_Chat.Internal
{
    internal class MessageStreamWriterFactory : IMessageStreamWriterFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        public MessageStreamWriterFactory( ILoggerFactory loggerFactory )
        {
            _loggerFactory = loggerFactory;
        }

        public IMessageStreamWriter Create( HttpResponse response )
        {
            return new MessageStreamWriter( response, _loggerFactory.CreateLogger<MessageStreamWriter>() );
        }
    }
}
