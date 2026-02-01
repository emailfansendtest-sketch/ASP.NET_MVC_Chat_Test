using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// Background worker that loads sensitive configuration (DB/SMTP) from HashiCorp Vault.
    /// It performs an initial load with retries until readiness is achieved and optionally
    /// refreshes secrets periodically if <see cref="SensitiveDataClientOptions.RefreshReadIntervalMs"/> is configured.
    /// </summary>
    internal class VaultSecretsLoadWorker : BackgroundService
    {
        private const int DefaultInitialIntervalMs = 5_000;

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

        /// <inheritdoc />
        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            var initialDelay = _options.InitialReadIntervalMs > 0 ?
                TimeSpan.FromMilliseconds( _options.InitialReadIntervalMs ) :
                TimeSpan.FromMilliseconds( DefaultInitialIntervalMs );

            TimeSpan? refreshDelay = _options.RefreshReadIntervalMs > 0 ?
                TimeSpan.FromMilliseconds( _options.RefreshReadIntervalMs ) :
                null;

            _logger.LogInformation(
                "VaultSecretsLoadWorker started. InitialInterval={InitialMs}ms, RefreshInterval={RefreshMs}ms.",
                initialDelay.TotalMilliseconds,
                refreshDelay?.TotalMilliseconds ?? 0 );

            var delay = initialDelay;

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

                if( isLoaded )
                {
                    if( !_readinessTracker.IsReady )
                    {
                        _readinessTracker.SignalReady();
                        _logger.LogInformation( "Initial Vault secrets load successful." );

                        // If the refresh read time interval is 0 - then refresh is disabled, the worker is safe.
                        if( refreshDelay == null )
                        {
                            _logger.LogInformation( "Vault secrets refresh is disabled (RefreshReadIntervalMs = 0)." );
                            _logger.LogInformation( "VaultSecretsLoadWorker stopped." );
                            break;
                        }    

                        delay = refreshDelay.Value;
                    }
                    else
                    {
                        _logger.LogInformation( "Vault secrets refresh successful." );
                    }
                }

                if( stoppingToken.IsCancellationRequested )
                {
                    break;
                }

                if( !_readinessTracker.IsReady )
                {
                    _logger.LogWarning( "Initial Vault load failed. Retrying in {Delay}ms.", delay.TotalMilliseconds );
                }

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

                await _vaultDataLoader.LoadAndCacheSecretsAsync( credentials!, ct );
                return true;
            }
            catch( OperationCanceledException ) when( ct.IsCancellationRequested )
            {
                // Expected during shutdown - no error logging is required
            }
            catch(Exception ex)
            {
                _logger.LogError( ex, "Error while loading secrets from Vault." );
            }
            return false;
        }
    }
}
