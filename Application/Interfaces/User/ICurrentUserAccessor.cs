
using System.Security.Claims;

namespace Application.Interfaces.User
{
    /// <summary>
    /// Provides access to the current request's <see cref="ClaimsPrincipal"/>.
    /// Intended for application-layer components that need to read user identity/claims
    /// without depending on ASP.NET Core primitives directly.
    /// </summary>
    internal interface ICurrentUserAccessor
    {
        /// <summary>
        /// The authenticated principal for the current request, or an empty principal if unauthenticated.
        /// </summary>
        ClaimsPrincipal Principal { get; }
    }
}
