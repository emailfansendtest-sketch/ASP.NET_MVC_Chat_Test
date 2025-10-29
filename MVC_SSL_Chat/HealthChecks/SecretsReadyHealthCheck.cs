using Contracts.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MVC_SSL_Chat.HealthChecks
{
    public sealed class SecretsReadyHealthCheck : IHealthCheck
    {
        private readonly ISecretsReadinessTracker _tracker;

        public SecretsReadyHealthCheck( ISecretsReadinessTracker tracker )
            => _tracker = tracker;

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default )
        {
            return Task.FromResult(
                _tracker.IsReady
                    ? HealthCheckResult.Healthy( "Secrets loaded" )
                    : HealthCheckResult.Unhealthy( "Secrets not loaded yet" ) );
        }
    }
}
