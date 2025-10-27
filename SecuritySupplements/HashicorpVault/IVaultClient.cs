
namespace SecuritySupplements.HashicorpVault
{
    internal interface IVaultClient
    {
        Task AccessAsync( VaultCredentials vaultOptions );
    }
}
