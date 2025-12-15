using Application.DTO;
using MVC_SSL_Chat.Models;

namespace MVC_SSL_Chat.Internal
{
    /// <summary>
    /// The helper class for converting the <see cref="ChatMessageDto"/> into the <see cref="MessageViewModel" />.
    /// </summary>
    public static class ModelsMappingExtensions
    {
        /// <summary>
        /// Converts the <see cref="ChatMessageDto" /> object into the <see cref="MessageViewModel" /> object.
        /// </summary>
        /// <param name="message">The original <see cref="ChatMessageDto" />.</param>
        /// <returns>The message converted to <see cref="MessageViewModel" />.</returns>
        public static MessageViewModel ToViewModel( this ChatMessageDto messageDto ) =>
            new MessageViewModel()
            {
                AuthorName = messageDto.AuthorName,
                Content = messageDto.Content,
                CreatedTime = messageDto.CreatedTime
            };
    }
}
