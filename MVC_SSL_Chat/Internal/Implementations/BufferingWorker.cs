using MVC_SSL_Chat.Internal.Interfaces;
using Storage.Contracts;

namespace MVC_SSL_Chat.Internal.Implementations
{
    public class BufferingWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBufferService _bufferService;
        private readonly int _savingFrequencyInMilliseconds;

        public BufferingWorker( ILoggerFactory loggerFactory, 
            IBufferService bufferService, 
            IStorageSettingsProvider settingsProvider )
        {
            _logger = loggerFactory.CreateLogger<BufferingWorker>();
            _bufferService = bufferService;
            _savingFrequencyInMilliseconds = settingsProvider.SavingFrequencyInMilliseconds;
        }

        protected override async Task ExecuteAsync( CancellationToken cancellationToken )
        {
            _logger.LogInformation( $"Buffering started, saving period in milliseconds:{ _savingFrequencyInMilliseconds }." );

            while( !cancellationToken.IsCancellationRequested )
            {
                await _bufferService.FlushAsync();

                // Waits for the time period obtained from the settings
                // before flushing the buffer
                await Task.Delay( TimeSpan.FromMilliseconds( _savingFrequencyInMilliseconds ), cancellationToken );
            }
        }
    }
}
