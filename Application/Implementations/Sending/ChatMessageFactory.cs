using Application.Exceptions;
using Application.Interfaces.Sending;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using DomainModels;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace Application.Implementations.Sending
{
    internal class ChatMessageFactory : IChatMessageFactory
    {
        private readonly IUserService _userService;
        private readonly IClock _clock;

        public ChatMessageFactory( IUserService userService, IClock clock ) 
        {
            _userService = userService;
            _clock = clock;
        }

        public async Task<ChatMessage> CreateAsync( string content )
        {
            var author = await _userService.GetCurrentUserAsync();

            if( author == null )
            {
                throw new UserNotFoundException();
            }

            var created = _clock.UtcNow;

            return new ChatMessage
            {
                Author = author!,
                Content = content,
                CreatedTime = created,
                AuthorId = author.Id
            };
        }
    }
}
