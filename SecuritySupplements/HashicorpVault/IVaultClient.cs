
namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// Loads sensitive configuration from HashiCorp Vault (e.g., DB and SMTP credentials)
    /// and updates the application's options caches so the rest of the application can
    /// consume the values through <c>IOptions&lt;T&gt;</c>/<c>IOptionsMonitor&lt;T&gt;</c>.
    /// </summary>
    internal interface IVaultClient
    {
        /// <summary>
        /// Reads secrets from Vault using the supplied credentials and populates the relevant
        /// <c>IOptionsMonitorCache&lt;T&gt;</c> entries (e.g., <c>DatabaseOptions</c>, <c>SmtpOptions</c>).
        /// </summary>
        /// <param name="credentials">Vault endpoint/token and secret path information.</param>
        Task LoadAndCacheSecretsAsync(
            VaultCredentials credentials,
            CancellationToken cancellationToken = default );
    }
}