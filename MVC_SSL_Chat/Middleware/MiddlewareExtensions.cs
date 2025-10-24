namespace MVC_SSL_Chat.Middleware
{
    public static class MiddlewareExtensions
    {
        public static void UseSecretsReadinessGate( this IApplicationBuilder app )
        {
            app.UseMiddleware<SecretsReadinessGateMiddleware>();
        }
        
    }
}
