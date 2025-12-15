using Application.Implementations.Utilities;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Implementations.Streaming
{
    internal class MessageStreamService : IMessageStreamService
    {
        private readonly IChatEventBus _eventBus;
        private readonly IDbService _dbService; // returns domain messages
        private readonly IClock _clock;
        private readonly MessageStreamOptions _messageStream;
        private readonly ILogger<MessageStreamService> _logger;

        public MessageStreamService(
            IChatEventBus eventBus,
            IDbService dbService,
            IClock clock,
            IOptions<MessageStreamOptions> messageStream,
            ILogger<MessageStreamService> logger )
        {
            _eventBus = eventBus;
            _dbService = dbService;
            _clock = clock;
            _messageStream = messageStream.Value;
            _logger = logger;
        }

        public async Task StreamForClientAsync(
            IMessageStreamWriter writer, CancellationToken ct )
        {
            _eventBus.Subscribe( writer );

            try
            {
                await SendMessagesAsync( writer, ct );

                // Keep-alive loop: can be in this service (clean)
                while(!ct.IsCancellationRequested)
                {
                    await writer.WriteKeepAliveAsync( ct );
                    await Task.Delay(
                        _messageStream.KeepAliveIntervalMs,
                        ct );
                }
            }
            catch(TaskCanceledException)
            {
                _logger.LogDebug( "Client token cancellation detected" );
            }
            finally
            {
                _eventBus.Unsubscribe( writer );
            }
        }

        private async Task SendMessagesAsync(
            IMessageStreamWriter writer, CancellationToken ct )
        {
            var to = _clock.UtcNow;
            var from = to.AddSeconds( - _messageStream.ReplayLookbackS );
            var messages = await _dbService.GetMessages( from, to ); // domain models
            foreach( var message in messages )
            {
                await writer.WriteMessageAsync( message.ToDto(), ct );
            }
        }
    }
}
