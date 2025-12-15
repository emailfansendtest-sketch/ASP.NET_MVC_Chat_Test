using Application.DTO;
using Application.Exceptions;
using Application.Interfaces.EntityCreation;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using DomainModels;

namespace Application.Implementations.EntityCreation
{
    internal class ChatMessageDtoFactory : IChatMessageDtoFactory
    {
        private readonly IUserService _userService;
        private readonly IClock _clock;

        public ChatMessageDtoFactory( IUserService userService, IClock clock )
        {
            _userService = userService;
            _clock = clock;
        }

        public async Task<ChatMessageDto> CreateAsync( string content )
        {
            var author = await _userService.GetCurrentUserAsync();

            if( author == null )
            {
                throw new UserNotFoundException();
            }

            if( string.IsNullOrEmpty(author!.UserName ) )
            {
                throw new InvalidUserException();
            }

            var created = _clock.UtcNow;

            return new ChatMessageDto
            {
                AuthorName = author!.UserName!,
                Content = content,
                CreatedTime = created,
                AuthorId = author.Id
            };
        }
    }
}
