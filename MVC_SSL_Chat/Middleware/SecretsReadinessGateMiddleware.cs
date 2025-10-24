using SecuritySupplements.Contracts;

namespace MVC_SSL_Chat.Middleware
{
    public class SecretsReadinessGateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecretsReadinessTracker _secretsReadinessTracker;

        public SecretsReadinessGateMiddleware( RequestDelegate next, ISecretsReadinessTracker secretsReadinessTracker )
        {
            _next = next;
            _secretsReadinessTracker = secretsReadinessTracker;
        }

        public async Task InvokeAsync( HttpContext context )
        {
            if( !_secretsReadinessTracker.IsReady && 
                !context.Request.Path.StartsWithSegments( "/health" ) )
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync( "Application not ready. Please try again later." );
                return;
            }

            await _next( context );
        }
    }
}
