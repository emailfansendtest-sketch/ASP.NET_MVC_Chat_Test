using VaultSharp.V1.AuthMethods.Token;
using VaultSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Contracts.Options;

namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// The implementation of the provider of the protection critical data.
    /// </summary>
    internal class HashicorpVaultClient : IVaultClient
    {
        private readonly ILogger<HashicorpVaultClient> _logger;

        private readonly IOptionsMonitorCache<DatabaseOptions> _databaseCache;
 
        private readonly IOptionsMonitorCache<SmtpOptions> _smtpCache;

        public HashicorpVaultClient( IOptionsMonitorCache<DatabaseOptions> databaseCache, 
            IOptionsMonitorCache<SmtpOptions> smtpCache, 
            ILogger<HashicorpVaultClient> logger )
        {
            _logger = logger;
            _databaseCache = databaseCache;
            _smtpCache = smtpCache;
        }

        public async Task AccessAsync( VaultCredentials vaultOptions )
        {
            var client = new VaultSharp.VaultClient( new VaultClientSettings(
                vaultOptions.Address, new TokenAuthMethodInfo( vaultOptions.Token ) ) );

            _logger.LogInformation( "Requesting vault secrets." );

            var secrets = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: vaultOptions.Path,
                mountPoint: vaultOptions.MountPoint );

            var data = secrets.Data.Data;

            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = data[ "DatabaseConnection" ].ToString()!
            };
            _databaseCache.TryRemove( nameof( DatabaseOptions ) );
            _databaseCache.TryAdd( nameof( DatabaseOptions ), databaseOptions );

            var smtpOptions = new SmtpOptions
            {
                Host = data[ "EmailSmtpHost" ].ToString()!,
                Port = int.Parse( data[ "EmailSmtpPort" ].ToString()! ),
                FromAddress = data[ "EmailAddress" ].ToString()!,
                FromPassword = data[ "EmailPassword" ].ToString()!,
                FromDisplayName = data.ContainsKey( "EmailDisplayName" ) ? data[ "EmailDisplayName" ].ToString()! : string.Empty
            };
            _smtpCache.TryRemove( nameof( SmtpOptions ) );
            _smtpCache.TryAdd( nameof( SmtpOptions ), smtpOptions );

            _logger.LogInformation( "Vault secrets have been read successfully." );
        }
    }
}
