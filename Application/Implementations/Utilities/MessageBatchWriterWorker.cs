using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterWorker : BackgroundService
    {
        private static readonly TimeSpan MaxRetryDelay =
            TimeSpan.FromSeconds(30);

        private readonly ILogger<MessageBatchWriterWorker> _logger;
        private readonly IMessageWriterService _messageWriterService;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;
        private readonly PersistenceOptions _options;
        private readonly ResiliencePipeline _flushRetryPipeline;

        public MessageBatchWriterWorker(
            ILogger<MessageBatchWriterWorker> logger,
            IMessageWriterService messageWriterService,
            ISecretsReadinessTracker secretsReadinessTracker,
            IOptions<PersistenceOptions> options)
        {
            _logger = logger;
            _messageWriterService = messageWriterService;
            _secretsReadinessTracker = secretsReadinessTracker;
            _options = options.Value;

            _flushRetryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(
                    new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder()
                            .Handle<Exception>(
                                ex => ex is not OperationCanceledException),

                        MaxRetryAttempts = int.MaxValue,

                        Delay = TimeSpan.FromMilliseconds(
                            _options.FlushIntervalMs),

                        BackoffType = DelayBackoffType.Exponential,

                        UseJitter = true,

                        MaxDelay = MaxRetryDelay,

                        OnRetry = args =>
                        {
                            _logger.LogWarning(
                                args.Outcome.Exception,
                                "Failed to flush buffered messages. " +
                                "Retry attempt {RetryAttempt} will run after {RetryDelay}.",
                                args.AttemptNumber + 1,
                                args.RetryDelay);

                            return default;
                        }
                    })
                .Build();
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Message batch writer worker started.");

            try
            {
                await _secretsReadinessTracker
                    .WaitUntilReadyAsync(stoppingToken);

                _logger.LogInformation(
                    "Required secrets are ready. " +
                    "Message persistence worker is active.");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await _flushRetryPipeline.ExecuteAsync(
                        async cancellationToken =>
                        {
                            await _messageWriterService.ProcessPendingMessagesAsync();
                        },
                        stoppingToken);

                    await Task.Delay(
                        TimeSpan.FromMilliseconds(
                            _options.FlushIntervalMs),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Message batch writer worker terminated unexpectedly.");

                throw;
            }
            finally
            {
                _logger.LogInformation(
                    "Message batch writer worker stopped.");
            }
        }
    }
}