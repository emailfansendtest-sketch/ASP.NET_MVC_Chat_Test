using Microsoft.AspNetCore.Antiforgery;

namespace MVC_SSL_Chat.Antiforgery
{
    public static class AntiforgeryExtensions
    {
        public static void AddCustomAntiforgery( this WebApplicationBuilder builder )
        {
            builder.Services.AddAntiforgery( o =>
            {
                o.HeaderName = "X-CSRF-TOKEN";
            } );
        }

        public static void MapCustomAntiforgery( this IEndpointRouteBuilder endpoints )
        {
            endpoints.MapGet( "/antiforgery/token", ( HttpContext http, IAntiforgery af ) =>
            {
                var tokens = af.GetAndStoreTokens( http ); // sets the cookie
                return Results.Json( new { token = tokens.RequestToken, header = "X-CSRF-TOKEN" } );
            } );
        }

    }
}
