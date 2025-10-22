using Application.Interfaces.User;
using DomainModels;

namespace Application.Implementations.User
{
    internal class UserService : IUserService
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;
        private readonly IUserRepository _userRepository;

        public UserService(
            ICurrentUserAccessor currentUserAccessor, IUserRepository userRepository ) 
        {
            _currentUserAccessor = currentUserAccessor;
            _userRepository = userRepository;
        }

        public async Task<ChatUser?> GetCurrentUserAsync()
        {
            var principal = _currentUserAccessor.Principal;
            return await _userRepository.GetByPrincipal( principal );
        }
    }
}
