using Application.Contracts;
using Application.Interfaces.Sending;
using Application.Interfaces.Utilities;
using DomainModels;

namespace Application.Implementations.Sending
{
    internal class MessageSenderService : IMessageSenderService
    {
        private readonly IChatEventBus _eventBus;
        private readonly IMessageBatchWriterService _batchWriter;
        private readonly IClock _clock;

        public MessageSenderService( 
            IChatEventBus eventBus, IMessageBatchWriterService batchWriter, IClock clock )
        {
            _eventBus = eventBus;
            _batchWriter = batchWriter;
            _clock = clock;
        }

        public async Task SendAsync( string content, ChatUser author, CancellationToken ct = default )
        {
            var created = _clock.UtcNow;

            var dto = new MessageDto
            {
                Author = author,
                Content = content,
                CreatedTime = created
            };

            // Buffer for bulk persisting
            _batchWriter.Append( dto );

            await _eventBus.PublishAsync( dto );
        }
    }
}
