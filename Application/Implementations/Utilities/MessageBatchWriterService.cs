using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

using Application.DTO;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterService : IMessageWriterService
    {
        private readonly Channel<ChatMessageDto> _channel;
        private readonly IDbService _dbService;
        private readonly MessageWriterOptions _options;
        private readonly ConcurrentQueue<ChatMessageDto> _batch = new();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="dbService">The database service.</param>
        public MessageBatchWriterService( 
            IDbService dbService, 
            IOptions<MessageWriterOptions> options )
        {
            _options = options.Value;

            _channel = Channel.CreateBounded<ChatMessageDto>(
                new BoundedChannelOptions( _options.MessageQueueCapacity )
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                }
                );

            _dbService = dbService;
        }

        /// <inheritdoc />
        public async Task AppendAsync( ChatMessageDto message, CancellationToken ct )
        {
            await _channel.Writer.WriteAsync( message, ct );
        }

        /// <inheritdoc />
        public async Task ProcessPendingMessagesAsync( )
        {
            if( !_batch.IsEmpty )
            {
                await FlushAsync( _batch ); // Saving previous message chunk if the sending has failed.
            }

            while( _channel.Reader.TryRead( out var msg ) )
            {
                _batch.Enqueue( msg );

                if( _batch.Count < _options.MessageBatchSize )
                {
                    continue; // The _batch size did not reach the maximum.
                }

                await FlushAsync( _batch );
            }

            if( !_batch.IsEmpty )
            {
                await FlushAsync( _batch ); // Saving last message chunk.
            }
        }

        public void Dispose()
        {
            // Signals the channel that there will be no messages past the ones already written;
            _channel.Writer.Complete();
        }

        private async Task FlushAsync( IEnumerable<ChatMessageDto> messages )
        {
            await _dbService.SaveChangesAsync( 
                messages.Select( EntitiesMappingExtensions.ToDomain ) 
                );
            _batch.Clear();
        }
    }
}
