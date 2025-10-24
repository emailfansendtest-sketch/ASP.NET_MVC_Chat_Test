
namespace SecuritySupplements.HashicorpVault
{
    internal interface IVaultDataLoader
    {
        Task LoadAsync( VaultCredentials vaultOptions );
    }
}
