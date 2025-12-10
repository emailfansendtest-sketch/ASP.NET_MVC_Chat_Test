using DomainModels;
using MVC_SSL_Chat.Models;

namespace MVC_SSL_Chat.Internal
{
    /// <summary>
    /// The helper class for converting the <see cref="ChatMessage"/> into the <see cref="MessageViewModel" />.
    /// </summary>
    public static class ModelsMappingExtensions
    {
        /// <summary>
        /// Converts the <see cref="ChatMessage" /> object into the <see cref="MessageViewModel" /> object.
        /// </summary>
        /// <param name="message">The original <see cref="ChatMessage" />.</param>
        /// <returns>The message converted to <see cref="MessageViewModel" />.</returns>
        public static MessageViewModel ToViewModel( this ChatMessage message ) =>
            new MessageViewModel()
            {
                AuthorName = message.Author.UserName,
                Content = message.Content,
                CreatedTime = message.CreatedTime
            };
    }
}
