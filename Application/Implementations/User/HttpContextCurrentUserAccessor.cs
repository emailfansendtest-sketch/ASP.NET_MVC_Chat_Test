using Application.Interfaces.User;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Implementations.User
{
    public class HttpContextCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCurrentUserAccessor( IHttpContextAccessor httpContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal Principal =>
            _httpContextAccessor.HttpContext?.User
                ?? new ClaimsPrincipal();
    }
}
