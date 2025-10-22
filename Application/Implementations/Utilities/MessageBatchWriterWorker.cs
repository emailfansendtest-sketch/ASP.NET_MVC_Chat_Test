using Application.Interfaces.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Storage.Contracts;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMessageBatchWriterService _bufferService;
        private readonly int _savingFrequencyInMilliseconds;

        public MessageBatchWriterWorker( ILoggerFactory loggerFactory,
            IMessageBatchWriterService bufferService,
            IStorageSettingsProvider settingsProvider )
        {
            _logger = loggerFactory.CreateLogger<MessageBatchWriterWorker>();
            _bufferService = bufferService;
            _savingFrequencyInMilliseconds = settingsProvider.SavingFrequencyInMilliseconds;
        }

        protected override async Task ExecuteAsync( CancellationToken cancellationToken )
        {
            _logger.LogInformation( $"Buffering started, saving period in milliseconds:{_savingFrequencyInMilliseconds}." );

            while(!cancellationToken.IsCancellationRequested)
            {
                await _bufferService.FlushAsync();

                // Waits for the time period obtained from the settings
                // before flushing the buffer
                await Task.Delay( TimeSpan.FromMilliseconds( _savingFrequencyInMilliseconds ), cancellationToken );
            }
        }
    }
}
