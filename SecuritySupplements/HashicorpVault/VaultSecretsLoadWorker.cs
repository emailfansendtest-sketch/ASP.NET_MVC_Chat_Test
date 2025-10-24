using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecuritySupplements.Contracts;

namespace SecuritySupplements.HashicorpVault
{
    internal class VaultSecretsLoadWorker : BackgroundService
    {
        private readonly ILogger<VaultSecretsLoadWorker> _logger;
        private readonly IVaultDataLoader _vaultDataLoader;
        private readonly ISecretsReadinessTracker _readinessTracker;
        private readonly IVaultCredentialsReader _credentialsReader;
        private readonly IReaderSettingsProvider _settings;

        public VaultSecretsLoadWorker(
            ILogger<VaultSecretsLoadWorker> logger,
            IVaultDataLoader vaultDataLoader,
            ISecretsReadinessTracker readinessTracker,
            IVaultCredentialsReader credentialsReader,
            IReaderSettingsProvider settings )
        {
            _logger = logger;
            _vaultDataLoader = vaultDataLoader;
            _readinessTracker = readinessTracker;
            _credentialsReader = credentialsReader;
            _settings = settings;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            _logger.LogInformation( "VaultSecretsLoadWorker started." );

            var delay = TimeSpan.FromMilliseconds( _settings.ReadingDelay );

            while( !stoppingToken.IsCancellationRequested )
            {
                if( !_readinessTracker.IsReady )
                {
                    _logger.LogInformation( "Loading initial Vault secrets..." );
                }
                else
                {
                    _logger.LogInformation( "Refreshing Vault secrets..." );
                }

                bool isLoaded = await TryLoadAsync( stoppingToken );

                if( isLoaded && !_readinessTracker.IsReady )
                {
                    _readinessTracker.SignalReady();
                    _logger.LogInformation( "Initial Vault secrets load successful." );
                    break;
                }

                if( stoppingToken.IsCancellationRequested )
                {
                    break;
                }

                _logger.LogWarning( "Initial Vault load failed. Retrying in {Delay}ms.", delay.TotalMilliseconds );
                await Task.Delay( delay, stoppingToken );
            }

            _logger.LogInformation( "VaultSecretsLoadWorker stopped." );
        }

        private async Task<bool> TryLoadAsync( CancellationToken ct )
        {
            try
            {
                var credentials = _credentialsReader.Read();

                if(credentials == null)
                {
                    _logger.LogError( "Vault credentials could not be read." );
                    return false;
                }

                await _vaultDataLoader.LoadAsync( credentials! );
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError( ex, "Error while loading secrets from Vault." );
            }
            return false;
        }
    }
}
