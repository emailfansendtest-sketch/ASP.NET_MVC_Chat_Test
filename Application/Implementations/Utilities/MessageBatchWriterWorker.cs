using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMessageBatchWriterService _batchWriterService;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;
        private readonly PersistenceOptions _persistence;
        public MessageBatchWriterWorker( ILogger<MessageBatchWriterWorker> logger,
            IMessageBatchWriterService batchWriterService,
            ISecretsReadinessTracker secretsReadinessTracker,
            IOptions<PersistenceOptions> persistence )
        {
            _logger = logger;
            _batchWriterService = batchWriterService;
            _secretsReadinessTracker = secretsReadinessTracker;
            _persistence = persistence.Value;
        }

        protected override async Task ExecuteAsync( CancellationToken cancellationToken )
        {
            _logger.LogInformation( $"Buffering started, saving period in milliseconds:{ _persistence.FlushIntervalMs }." );

            while(!cancellationToken.IsCancellationRequested)
            {
                await _secretsReadinessTracker.WaitUntilReadyAsync( cancellationToken );

                await _batchWriterService.FlushAsync();

                // Waits for the time period obtained from the settings
                // before flushing the buffer
                await Task.Delay( TimeSpan.FromMilliseconds( _persistence.FlushIntervalMs ), cancellationToken );
            }
        }
    }
}
