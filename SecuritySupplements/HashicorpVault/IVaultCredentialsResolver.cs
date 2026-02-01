
namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// Reads the vault access credentials depending on the current runtime environment
    /// </summary>
    internal interface IVaultCredentialsResolver
    {
        /// <summary>
        /// Read the Hashicorp Vault access credentials.
        /// </summary>
        /// <returns>If the credentials were successfully read - wrapper for Vault credentials. Null otherwise.</returns>
        VaultCredentials? Resolve();
    }
}
