using Application.DTO;
using Application.Interfaces.Streaming;

using System.Text.Json;

namespace MVC_SSL_Chat.Internal
{
    internal class MessageStreamWriter : IMessageStreamWriter
    {
        private readonly HttpResponse _response;
        private readonly ILogger<MessageStreamWriter> _logger;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim( 1, 1 );

        public MessageStreamWriter( HttpResponse response, ILogger<MessageStreamWriter> logger )
        {
            _response = response;
            _logger = logger;
        }

        public async Task WriteMessageAsync( ChatMessageDto messageDto, CancellationToken ct = default )
        {
            var json = JsonSerializer.Serialize( messageDto.ToViewModel() );
            var formatted = $"data: {json}\n\n";
            await WriteNonInterleaved( formatted, ct );
            _logger.LogTrace( "Streamed message to client" );
        }

        public async Task WriteKeepAliveAsync( CancellationToken ct = default )
        {
            try
            {
                await WriteNonInterleaved( ": keep-alive\n\n", ct );
            }
            catch(Exception ex)
            {
                _logger.LogDebug( ex, "Keep-alive failed (client disconnected?)" );
            }

        }
        private async Task WriteNonInterleaved( string payload, CancellationToken ct )
        {
            await _writeLock.WaitAsync( ct );
            try
            {
                await _response.WriteAsync( payload, ct );
                await _response.Body.FlushAsync( ct );
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}
