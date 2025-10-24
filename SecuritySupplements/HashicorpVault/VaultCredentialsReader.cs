using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// TODO remove all explicit configuration reading - configuration pattern should be used instead.
    /// </summary>
    internal class VaultCredentialsReader : IVaultCredentialsReader
    {
        private const string ConfigurationUserSecretsSectionName = "HashicorpVault";
        private const string CredentialToken = "HashicorpVaultCredentialToken";
        private const string CredentialAddress = "HashicorpVaultCredentialAddress";
        private const string CredentialPath = "HashicorpVaultCredentialPath";
        private const string CredentialMountPoint = "HashicorpVaultCredentialMountPoint";

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public VaultCredentialsReader( ILogger<VaultCredentialsReader> logger,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment )
        {
            _logger = logger;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Read the Hashicorp Vault access credentials.
        /// </summary>
        /// <returns>If the credentials were successfully read - wrapper for Vault credentials. Null otherwise.</returns>
        public VaultCredentials? Read()
        {
            return _hostEnvironment.IsDevelopment() ?
                ReadFromUserSecrets() :
                ReadFromEnvironmentVariables();
        }

        private VaultCredentials? ReadFromUserSecrets()
        {
            try
            {
                _logger.LogTrace( $"Attempting to read the {ConfigurationUserSecretsSectionName} configuration section." );
                var section = _configuration.GetSection( ConfigurationUserSecretsSectionName );

                if(!section.Exists())
                {
                    _logger.LogCritical( $"Configuration section {ConfigurationUserSecretsSectionName} does not exist! Application cannot start." );
                    return null;
                }
                var result = section.Get<VaultCredentials>()!;

                _logger.LogTrace( $"Configuration section {ConfigurationUserSecretsSectionName} was successfully read." );
                return result;
            }
            catch(Exception ex)
            {
                _logger.LogCritical( ex, $"Error reading the configuration section {ConfigurationUserSecretsSectionName}. See exception details. Application cannot start." );
                return null;
            }
        }

        private VaultCredentials? ReadFromEnvironmentVariables()
        {
            VaultCredentials vaultCredentials = new VaultCredentials();

            _logger.LogTrace( $"Attempting to read the environment variables." );

            if(!SetFromEnvironmentValue( CredentialToken, v => vaultCredentials.Token = v ))
            {
                return null;
            }

            if(!SetFromEnvironmentValue( CredentialAddress, v => vaultCredentials.Address = v ))
            {
                return null;
            }

            if(!SetFromEnvironmentValue( CredentialPath, v => vaultCredentials.Path = v ))
            {
                return null;
            }

            if(!SetFromEnvironmentValue( CredentialMountPoint, v => vaultCredentials.MountPoint = v ))
            {
                return null;
            }

            _logger.LogTrace( $"The environment variables were read successfully." );

            return vaultCredentials;
        }

        private bool SetFromEnvironmentValue( string environmentVariableName, Action<string?> setEnvironmentVariableValue )
        {
            try
            {
                _logger.LogTrace( $"Attempting to read the environment variable {environmentVariableName}." );

                var value = Environment.GetEnvironmentVariable( environmentVariableName );

                _logger.LogTrace( $"Environment variable {environmentVariableName} was read." );

                setEnvironmentVariableValue( value );

                _logger.LogTrace( $"Environment variable {environmentVariableName} was set." );

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogCritical( ex, $"Error reading the environment variable {environmentVariableName}. See exception details. Application cannot start." );

                return false;
            }
        }
    }
}