using Application.Interfaces.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecuritySupplements.Contracts;
using Storage.Contracts;
using System.Configuration;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMessageBatchWriterService _bufferService;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;
        private readonly int _savingFrequencyInMilliseconds;

        public MessageBatchWriterWorker( ILogger<MessageBatchWriterWorker> logger,
            IMessageBatchWriterService bufferService,
            IStorageSettingsProvider settingsProvider,
            ISecretsReadinessTracker secretsReadinessTracker )
        {
            _logger = logger;
            _bufferService = bufferService;
            _secretsReadinessTracker = secretsReadinessTracker;
            _savingFrequencyInMilliseconds = settingsProvider.SavingFrequencyInMilliseconds;
        }

        protected override async Task ExecuteAsync( CancellationToken cancellationToken )
        {
            _logger.LogInformation( $"Buffering started, saving period in milliseconds:{_savingFrequencyInMilliseconds}." );

            while(!cancellationToken.IsCancellationRequested)
            {
                await _secretsReadinessTracker.WaitUntilReadyAsync( cancellationToken );

                await _bufferService.FlushAsync();

                // Waits for the time period obtained from the settings
                // before flushing the buffer
                await Task.Delay( TimeSpan.FromMilliseconds( _savingFrequencyInMilliseconds ), cancellationToken );
            }
        }
    }
}
