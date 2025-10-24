using Microsoft.Extensions.Diagnostics.HealthChecks;
using SecuritySupplements.Contracts;

namespace MVC_SSL_Chat.HealthCheck
{
    internal class VaultHealthCheck : IHealthCheck

    {
        private readonly ISecretsReadinessTracker _readiness;

        public VaultHealthCheck( ISecretsReadinessTracker readiness )
        {
            _readiness = readiness;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default )
        {
            if(_readiness.IsReady)
                return Task.FromResult( HealthCheckResult.Healthy( "Vault secrets loaded." ) );
            else
                return Task.FromResult( HealthCheckResult.Healthy( "Vault secrets not yet loaded." ) );
        }
    }
}
