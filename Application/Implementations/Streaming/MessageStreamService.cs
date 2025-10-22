using Application.Contracts;
using Application.Interfaces.Streaming;
using Application.Interfaces.Utilities;
using Microsoft.Extensions.Logging;
using Storage.Contracts;

namespace Application.Implementations.Streaming
{
    internal class MessageStreamService : IMessageStreamService
    {
        private readonly IChatEventBus _eventBus;
        private readonly IDbService _dbService; // returns domain messages
        private readonly ILogger<MessageStreamService> _logger;
        private readonly IStorageSettingsProvider _storageSettingsProvider;

        public MessageStreamService(
            IChatEventBus eventBus,
            IDbService dbService,
            IStorageSettingsProvider storageSettingsProvider,
            ILogger<MessageStreamService> logger )
        {
            _eventBus = eventBus;
            _dbService = dbService;
            _storageSettingsProvider = storageSettingsProvider;
            _logger = logger;
        }

        public async Task StreamForClientAsync(
            IMessageStreamWriter writer, CancellationToken ct )
        {
            _eventBus.Subscribe( writer );

            try
            {
                await SendMessagesPerLastDayAsync( writer, ct );

                // Keep-alive loop: can be in this service (clean)
                while(!ct.IsCancellationRequested)
                {
                    await writer.WriteKeepAliveAsync( ct );
                    await Task.Delay(
                        _storageSettingsProvider.RefreshingFrequencyInMilliseconds, ct );
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

        private async Task SendMessagesPerLastDayAsync(
            IMessageStreamWriter writer, CancellationToken ct )
        {
            var to = DateTime.UtcNow;
            var from = to.AddDays( -1 );
            var rawMessages = await _dbService.GetMessages( from, to ); // domain models
            foreach(var raw in rawMessages)
            {
                var dto = DomainToDto( raw );
                await writer.WriteMessageAsync( dto, ct );
            }
        }

        /// <summary>
        /// TODO replace
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private MessageDto DomainToDto( DomainModels.ChatMessage raw )
            => new MessageDto
            {
                Author = raw.Author!,
                Content = raw.Content,
                CreatedTime = raw.CreatedTime
            };
    }
}
