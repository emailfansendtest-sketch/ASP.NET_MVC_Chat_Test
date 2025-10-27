
namespace SecuritySupplements.HashicorpVault
{
    /// <summary>
    /// Reads the vault access credentials depending on the current runtime environment
    /// </summary>
    internal interface IVaultCredentialsResolver
    {
        VaultCredentials? Resolve();
    }
}
