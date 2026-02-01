using Contracts.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;

namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// HashiCorp Vault KV v2 client that reads sensitive values (DB/SMTP)
    /// and updates <see cref="IOptionsMonitorCache{TOptions}"/> so application components can
    /// access the configuration via <see cref="IOptions{TOptions}"/>/<see cref="IOptionsMonitor{TOptions}"/>.
    /// </summary>
    internal class HashicorpVaultClient : IVaultClient
    {
        private readonly ILogger<HashicorpVaultClient> _logger;
        private readonly IOptionsMonitorCache<DatabaseOptions> _databaseCache;
        private readonly IOptionsMonitorCache<SmtpOptions> _smtpCache;

        public HashicorpVaultClient(
            IOptionsMonitorCache<DatabaseOptions> databaseCache,
            IOptionsMonitorCache<SmtpOptions> smtpCache,
            ILogger<HashicorpVaultClient> logger )
        {
            _logger = logger;
            _databaseCache = databaseCache;
            _smtpCache = smtpCache;
        }

        /// <summary>
        /// Reads the secret document from Vault KV v2 and updates cached <see cref="DatabaseOptions"/>
        /// and <see cref="SmtpOptions"/> values.
        /// </summary>
        public async Task LoadAndCacheSecretsAsync(
            VaultCredentials credentials,
            CancellationToken cancellationToken = default )
        {
            _logger.LogInformation( "Vault secrets read starting..." );
            if(credentials is null)
            {
                throw new ArgumentNullException( nameof( credentials ) );
            }
            if(string.IsNullOrWhiteSpace( credentials.Address ))
            {
                throw new ArgumentException( "Vault address is required.", nameof( credentials ) );
            }
            if(string.IsNullOrWhiteSpace( credentials.Token ))
            {
                throw new ArgumentException( "Vault token is required.", nameof( credentials ) );
            }
            if(string.IsNullOrWhiteSpace( credentials.Path ))
            {
                throw new ArgumentException( "Vault secret path is required.", nameof( credentials ) );
            }
            if(string.IsNullOrWhiteSpace( credentials.MountPoint ))
            {
                throw new ArgumentException( "Vault mount point is required.", nameof( credentials ) );
            }
            try
            {
                await LoadAndCacheSecretsInternalAsync( credentials, cancellationToken );
            }
            catch( VaultApiException ex )
            {
                var apiErrors = string.Join( "', '", ex.ApiErrors );
                var innerException = !string.IsNullOrEmpty( ex.InnerException?.Message ) ? ex.InnerException.Message : "None";
                _logger.LogError( "Failed to acquire the Vault secrets from mount '{MountPoint}', path '{Path}'.", credentials.MountPoint, credentials.Path );
                throw new Exception( $"Vault client error: '{ ex.Message }', api errors: '{ apiErrors }', inner exception:'{ innerException }'" );
            }


            _logger.LogInformation( "Vault secrets have been read and cached successfully." );
        }

        private async Task LoadAndCacheSecretsInternalAsync(
            VaultCredentials credentials,
            CancellationToken cancellationToken )
        {
            var client = new VaultSharp.VaultClient(
                new VaultClientSettings(
                    credentials.Address,
                    new TokenAuthMethodInfo( credentials.Token ) ) );

            _logger.LogInformation( "Requesting Vault secrets from mount '{MountPoint}', path '{Path}'.", credentials.MountPoint, credentials.Path );

            // KV v2: the "data" field contains your user-defined secret key/values.

            var readSecretTask = client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: credentials.Path,
                mountPoint: credentials.MountPoint );

            var readSecret = await readSecretTask.WaitAsync( cancellationToken );

            if( cancellationToken.IsCancellationRequested )
            {
                return;
            }

            var data = readSecret.Data?.Data;

            if(data is null)
            {
                throw new InvalidOperationException( "Vault returned an empty secret payload." );
            }

            string? GetOptionalString( IDictionary<string, object> dict, string key )
            {
                if(!dict.TryGetValue( key, out var value ) || value is null) return null;
                var s = value.ToString();
                return string.IsNullOrWhiteSpace( s ) ? null : s;
            }

            var dbConnection = GetRequiredString( data, "DatabaseConnection" );

            var smtpHost = GetRequiredString( data, "EmailSmtpHost" );
            var smtpPortRaw = GetRequiredString( data, "EmailSmtpPort" );
            if(!int.TryParse( smtpPortRaw, out var smtpPort ))
                throw new InvalidOperationException( $"Vault secret 'EmailSmtpPort' must be an integer, got '{smtpPortRaw}'." );

            var fromAddress = GetRequiredString( data, "EmailAddress" );
            var fromPassword = GetRequiredString( data, "EmailPassword" );
            var fromDisplayName = GetOptionalString( data, "EmailDisplayName" ) ?? string.Empty;

            // Update caches (replace the default named options entry).
            _databaseCache.TryRemove( Options.DefaultName );
            _databaseCache.TryAdd( Options.DefaultName, new DatabaseOptions
            {
                ConnectionString = dbConnection
            } );

            _smtpCache.TryRemove( Options.DefaultName );
            _smtpCache.TryAdd( Options.DefaultName, new SmtpOptions
            {
                Host = smtpHost,
                Port = smtpPort,
                FromAddress = fromAddress,
                FromPassword = fromPassword,
                FromDisplayName = fromDisplayName
            } );
        }

        private string GetRequiredString( IDictionary<string, object> dict, string key )
        {
            if(!dict.TryGetValue( key, out var value ) || value is null)
                throw new KeyNotFoundException( $"Required Vault secret key '{key}' was not found." );

            var s = value.ToString();
            if(string.IsNullOrWhiteSpace( s ))
                throw new InvalidOperationException( $"Vault secret key '{key}' is empty." );

            return s;
        }
    }
}
