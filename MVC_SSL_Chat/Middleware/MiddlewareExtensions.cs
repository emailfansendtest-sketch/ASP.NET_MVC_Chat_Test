namespace MVC_SSL_Chat.Middleware
{
    public static class MiddlewareExtensions
    {
        public static void UseCustomMiddleware( this IApplicationBuilder app )
        {
            app.UseMiddleware<SecretsReadinessGateMiddleware>();
        }
        
    }
}
