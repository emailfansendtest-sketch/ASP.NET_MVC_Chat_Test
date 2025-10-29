using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MVC_SSL_Chat.HealthChecks
{
    public sealed class LiveHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default )
            => Task.FromResult( HealthCheckResult.Healthy( "Service is alive" ) );
    }
}
