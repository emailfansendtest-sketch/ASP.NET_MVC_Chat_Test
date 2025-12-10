using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMessageWriterService _batchWriterService;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;
        private readonly PersistenceOptions _persistence;
        public MessageBatchWriterWorker( ILogger<MessageBatchWriterWorker> logger,
            IMessageWriterService batchWriterService,
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
            _logger.LogInformation( 
                "Buffered messages flush started, saving period in milliseconds:{0}.", _persistence.FlushIntervalMs );

            var sw = Stopwatch.StartNew();

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _secretsReadinessTracker.WaitUntilReadyAsync( cancellationToken );

                    await _batchWriterService.FlushAsync();

                    // Waits for the time period obtained from the settings
                    // before flushing the buffer
                    await Task.Delay( TimeSpan.FromMilliseconds( _persistence.FlushIntervalMs ), cancellationToken );
                    
                    sw.Stop();
                    _logger.LogInformation( 
                        "Buffered messages flush finished, elapsed milliseconds:{0}.", sw.ElapsedMilliseconds );
                }
                catch( Exception ex )
                {
                    sw.Stop();
                    _logger.LogError( ex, 
                        "Error while flushing buffered messages, elapsed milliseconds:{0}.", sw.ElapsedMilliseconds );
                }
                sw.Restart();
            }
        }
    }
}
