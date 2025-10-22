using Application.Contracts;
using DomainModels;
using MVC_SSL_Chat.Models;

namespace MVC_SSL_Chat.Internal
{
    /// <summary>
    /// TODO replace with AutoMapper
    /// The helper class for converting the <see cref="MessageDto"/>, <see cref="ChatMessage"/>, 
    /// <see cref="ChatUser"/> and <see cref="MessageViewModel" /> into each other.
    /// </summary>
    public static class MvcDataConverterHelper
    {
        /// <summary>
        /// Converts the <see cref="ChatMessage" /> object into the <see cref="MessageViewModel" /> object.
        /// </summary>
        /// <param name="message">The original <see cref="MessageDto" /> object.</param>
        /// <returns>The object converted to <see cref="MessageViewModel" />.</returns>
        public static MessageViewModel ToViewModel( this MessageDto message ) =>
            new MessageViewModel()
            {
                AuthorName = message.Author!.UserName,
                Content = message.Content,
                CreatedTime = message.CreatedTime
            };

        /// <summary>
        /// Converts the <see cref="MessageViewModel" /> object to the <see cref="MessageDto" /> object.
        /// </summary>
        /// <param name="messageViewModel">The original object.</param>
        /// <returns>The converted object.</returns>
        public static MessageDto ToMessageDto( this MessageViewModel messageViewModel, ChatUser author )
        {
            var dateTime = messageViewModel.CreatedTime!.Value;

            return new MessageDto
            {
                Content = messageViewModel.Content!,
                CreatedTime = dateTime,
                Author = author
            };
        }

        /// <summary>
        /// Converts the <see cref="ChatMessage" /> object into the <see cref="MessageDto" /> object.
        /// </summary>
        /// <param name="message">The original <see cref="ChatMessage" /> object.</param>
        /// <returns>The object converted to <see cref="MessageDto" />.</returns>
        public static ChatMessage ToDatabaseEntity( this MessageDto message )
        {
            return new ChatMessage
            {
                Content = message.Content,
                CreatedTime = message.CreatedTime,
                AuthorId = message.Author.Id,
                Author = message.Author
            };
        }
    }
}
