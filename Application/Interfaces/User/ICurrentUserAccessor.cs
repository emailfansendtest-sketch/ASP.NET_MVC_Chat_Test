
using System.Security.Claims;

namespace Application.Interfaces.User
{
    internal interface ICurrentUserAccessor
    {
        ClaimsPrincipal Principal { get; }
    }
}
