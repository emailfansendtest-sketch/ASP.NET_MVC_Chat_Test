using Application.Interfaces.Sending;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using DomainModels;

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
            var created = _clock.UtcNow;

            return new ChatMessage
            {
                Author = author,
                Content = content,
                CreatedTime = created
            };
        }
    }
}
