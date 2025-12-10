using DomainModels;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;

using System.Collections.Concurrent;
using System.Threading.Channels;
using Contracts.Options;
using Microsoft.Extensions.Options;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterService : IMessageWriterService
    {
        private readonly Channel<ChatMessage> _channel;
        private readonly IDbService _dbService;
        private readonly MessageWriterOptions _options;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="dbService">The database service.</param>
        public MessageBatchWriterService( IDbService dbService, IOptions<MessageWriterOptions> options )
        {
            _options = options.Value;

            _channel = Channel.CreateBounded<ChatMessage>(
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
        public async Task AppendAsync( ChatMessage message, CancellationToken ct )
        {
            await _channel.Writer.WriteAsync( message, ct );
        }

        /// <inheritdoc />
        public async Task FlushAsync( )
        {
            var batch = new ConcurrentQueue<ChatMessage>();

            while( _channel.Reader.TryRead( out var msg ) )
            {
                batch.Enqueue( msg );

                if(batch.Count < _options.MessageBatchSize )
                {
                    continue; // The batch size did not reach the maximum.
                }

                await SaveMessagesAsync( batch );
                batch.Clear();
            }

            if(batch.Count > 0)
            {
                await SaveMessagesAsync( batch ); // Saving last message chunk.
            }
        }

        public void Dispose()
        {
            // Signals the channel that there will be no messages past the ones already written;
            _channel.Writer.Complete();
        }

        private async Task SaveMessagesAsync( IEnumerable<ChatMessage> messages )
        {
            await _dbService.SaveChangesAsync( messages );
        }
    }
}
