using Application.Contracts;
using Application.Interfaces.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MVC_SSL_Chat.Internal
{
    internal class MessageStreamWriter : IMessageStreamWriter
    {
        private readonly HttpResponse _response;
        private readonly ILogger<MessageStreamWriter> _logger;

        public MessageStreamWriter( HttpResponse response, ILogger<MessageStreamWriter> logger )
        {
            _response = response;
            _logger = logger;
        }

        public async Task WriteMessageAsync( MessageDto message, CancellationToken ct = default )
        {
            var json = JsonSerializer.Serialize( message.ToViewModel() );
            var formatted = $"data: {json}\n\n";
            await _response.WriteAsync( formatted, ct );
            await _response.Body.FlushAsync( ct );
            _logger.LogTrace( "Streamed message to client" );
        }

        public async Task WriteKeepAliveAsync( CancellationToken ct = default )
        {
            try
            {
                await _response.WriteAsync( ": keep-alive\n\n", ct );
                await _response.Body.FlushAsync( ct );
            }
            catch(Exception ex)
            {
                _logger.LogDebug( ex, "Keep-alive failed (client disconnected?)" );
            }
        }
    }
}
