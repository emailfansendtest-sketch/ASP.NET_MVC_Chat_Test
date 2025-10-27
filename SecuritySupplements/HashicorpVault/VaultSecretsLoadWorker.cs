using Contracts.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MVC_SSL_Chat.Internal;

namespace SecuritySupplements.HashicorpVault
{
    internal class VaultSecretsLoadWorker : BackgroundService
    {
        private readonly ILogger<VaultSecretsLoadWorker> _logger;
        private readonly IVaultClient _vaultDataLoader;
        private readonly ISecretsReadinessTracker _readinessTracker;
        private readonly IVaultCredentialsResolver _credentialsReader;
        private readonly SensitiveDataClientOptions _options;

        public VaultSecretsLoadWorker(
            ILogger<VaultSecretsLoadWorker> logger,
            IVaultClient vaultDataLoader,
            ISecretsReadinessTracker readinessTracker,
            IVaultCredentialsResolver credentialsReader,
            IOptions<SensitiveDataClientOptions> options )
        {
            _logger = logger;
            _vaultDataLoader = vaultDataLoader;
            _readinessTracker = readinessTracker;
            _credentialsReader = credentialsReader;
            _options = options.Value;

        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            _logger.LogInformation( "VaultSecretsLoadWorker started." );

            var delay = TimeSpan.FromMilliseconds( _options.ConsequentReadDelayMs );

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
                var credentials = _credentialsReader.Resolve();

                if(credentials == null)
                {
                    _logger.LogError( "Vault credentials could not be read." );
                    return false;
                }

                await _vaultDataLoader.AccessAsync( credentials! );
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
