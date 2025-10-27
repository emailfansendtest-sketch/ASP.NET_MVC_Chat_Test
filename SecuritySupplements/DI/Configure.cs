using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.DependencyInjection;
using SecuritySupplements.HashicorpVault;

namespace SecuritySupplements.DI
{
    public static class Configure
    {
        public static IServiceCollection AddSecurityLayer( this IServiceCollection services )
        {
            services.AddOptions<SmtpOptions>();
            services.AddOptions<DatabaseOptions>();
            services.AddSingleton<IVaultCredentialsResolver, VaultCredentialsResolver>();
            services.AddSingleton<HashicorpVaultClient>();
            services.AddSingleton<IVaultClient, HashicorpVaultClient>();
            services.AddSingleton<ISecretsReadinessTracker, SecretsReadinessTracker>();
            services.AddHostedService<VaultSecretsLoadWorker>();

            return services;
        }
    }
}
