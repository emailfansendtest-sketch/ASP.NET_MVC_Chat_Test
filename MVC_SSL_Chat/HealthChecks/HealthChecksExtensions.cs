using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace MVC_SSL_Chat.HealthChecks
{
    public static class HealthChecksExtensions
    {
        public static void AddCustomHealthChecks( this WebApplicationBuilder builder )
        {
            builder.Services.AddHealthChecks()
                .AddCheck<LiveHealthCheck>( "live", tags: new[] { "live" } )
                .AddCheck<SecretsReadyHealthCheck>( "ready", tags: new[] { "ready" } );
        }

        public static void MapCustomHealthChecks( this IEndpointRouteBuilder endpoints )
        {
            endpoints.MapHealthChecks( "/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains( "live" )
            } );

            endpoints.MapHealthChecks( "/health/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains( "ready" )
            } );
        }
    }
}
