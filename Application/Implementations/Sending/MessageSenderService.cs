using Application.Interfaces.ChatEvents;
using Application.Interfaces.EntityCreation;
using Application.Interfaces.Sending;
using Application.Interfaces.Utilities;
using DomainModels;

namespace Application.Implementations.Sending
{
    internal class MessageSenderService : IMessageSenderService
    {
        private readonly IChatEventBus _eventBus;
        private readonly IMessageWriterService _batchWriter;
		private readonly IChatMessageDtoFactory _chatMessageDtoFactory;

		public MessageSenderService( 
            IChatEventBus eventBus, IMessageWriterService batchWriter, IChatMessageDtoFactory chatMessageDtoFactory )
        {
            _eventBus = eventBus;
            _batchWriter = batchWriter;
            _chatMessageDtoFactory = chatMessageDtoFactory;
		}

        public async Task SendAsync( string content, CancellationToken ct = default )
        {
            var message = await _chatMessageDtoFactory.CreateAsync( content );
			
            // Buffer for bulk persisting
			await _batchWriter.AppendAsync( message, ct );

            await _eventBus.PublishAsync( message );
        }
    }
}
