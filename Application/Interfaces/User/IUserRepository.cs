using System.Security.Claims;

using DomainModels;

namespace Application.Interfaces.User
{
    /// <summary>
    /// Read-only access to user data required by the application layer.
    /// </summary>
    internal interface IUserRepository
    {
        /// <summary>
        /// Resolves a <see cref="ChatUser"/> from a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal obtained from the current request.</param>
        /// <returns>
        /// The corresponding <see cref="ChatUser"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<ChatUser?> GetByPrincipal( ClaimsPrincipal principal );
    }
}
