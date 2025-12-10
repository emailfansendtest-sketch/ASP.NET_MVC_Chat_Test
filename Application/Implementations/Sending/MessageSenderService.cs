using Application.Interfaces.ChatEvents;
using Application.Interfaces.Sending;
using Application.Interfaces.Utilities;
using DomainModels;

namespace Application.Implementations.Sending
{
    internal class MessageSenderService : IMessageSenderService
    {
        private readonly IChatEventBus _eventBus;
        private readonly IMessageWriterService _batchWriter;
		private readonly IChatMessageFactory _chatMessageFactory;

		public MessageSenderService( 
            IChatEventBus eventBus, IMessageWriterService batchWriter, IChatMessageFactory chatMessageFactory )
        {
            _eventBus = eventBus;
            _batchWriter = batchWriter;
            _chatMessageFactory = chatMessageFactory;
		}

        public async Task SendAsync( string content, CancellationToken ct = default )
        {
            var message = await _chatMessageFactory.CreateAsync( content );
			
            // Buffer for bulk persisting
			await _batchWriter.AppendAsync( message, ct );

            await _eventBus.PublishAsync( message );
        }
    }
}
