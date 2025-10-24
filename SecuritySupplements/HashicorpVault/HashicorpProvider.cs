using SecuritySupplements.Contracts;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp;
using SecuritySupplements.HashicorpVault;
using Microsoft.Extensions.Logging;

namespace SecuritySupplements
{
    /// <summary>
    /// The implementation of the provider of the protection critical data.
    /// </summary>
    internal class HashicorpProvider : ISensitiveDataProvider, IVaultDataLoader
    {
        private readonly ILogger<HashicorpProvider> _logger;

        public string DatabaseConnectionString { get; private set; } = string.Empty;

        public string ConfirmationEmailSmtpHost { get; private set; } = string.Empty;

        public int ConfirmationEmailSmtpPort { get; private set; }

        public string ConfirmationEmailAddress { get; private set; } = string.Empty;

        public string ConfirmationEmailPassword { get; private set; } = string.Empty;

        public event EventHandler? SecretsReloaded;

        public HashicorpProvider( ILoggerFactory loggerFactory )
        {
            _logger = loggerFactory.CreateLogger<HashicorpProvider>();
        }

        public async Task LoadAsync( VaultCredentials vaultOptions )
        {
            var client = new VaultClient( new VaultClientSettings(
                vaultOptions.Address, new TokenAuthMethodInfo( vaultOptions.Token ) ) );

            _logger.LogInformation( "Requesting vault secrets." );

            var secrets = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: vaultOptions.Path,
            mountPoint: vaultOptions.MountPoint );

            var data = secrets.Data.Data;

            DatabaseConnectionString = data[ "DatabaseConnection" ].ToString()!;

            ConfirmationEmailSmtpHost = data[ "EmailSmtpHost" ].ToString()!;

            ConfirmationEmailSmtpPort = int.Parse( data[ "EmailSmtpPort" ].ToString()! );

            ConfirmationEmailAddress = data[ "EmailAddress" ].ToString()!;

            ConfirmationEmailPassword = data[ "EmailPassword" ].ToString()!;


            _logger.LogInformation( "Vault secrets have been read successfully." );

            SecretsReloaded?.Invoke( this, EventArgs.Empty );
        }
    }
}
