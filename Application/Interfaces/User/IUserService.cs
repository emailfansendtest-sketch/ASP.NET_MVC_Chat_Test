using DomainModels;

namespace Application.Interfaces.User
{
    /// <summary>
    /// High-level user operations needed by the application layer,
    /// such as resolving the current authenticated user entity.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets the current authenticated <see cref="ChatUser"/> for the active request context.
        /// Returns <c>null</c> if no user is authenticated.
        /// </summary>
        Task<ChatUser?> GetCurrentUserAsync();
    }
}
