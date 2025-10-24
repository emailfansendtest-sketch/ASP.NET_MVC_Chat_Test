using Microsoft.Extensions.DependencyInjection;
using SecuritySupplements;
using SecuritySupplements.Contracts;
using SecuritySupplements.HashicorpVault;
using Storage.Contracts;

namespace SecuritySupplements.DI
{
    public static class Configure
    {
        public static IServiceCollection AddSecurityLayer( this IServiceCollection services )
        {
            services.AddSingleton<IStorageSettingsProvider, NonSensitiveSettingsProvider>();
            services.AddSingleton<IReaderSettingsProvider, NonSensitiveSettingsProvider>();
            services.AddSingleton<IVaultCredentialsReader, VaultCredentialsReader>();
            services.AddSingleton<HashicorpProvider>();
            services.AddSingleton<IVaultDataLoader>( provider => provider.GetRequiredService<HashicorpProvider>() );
            services.AddSingleton<ISensitiveDataProvider>( provider => provider.GetRequiredService<HashicorpProvider>() );
            services.AddSingleton<ISecretsReadinessTracker, SecretsReadinessTracker>();
            services.AddHostedService<VaultSecretsLoadWorker>();

            return services;
        }
    }
}
