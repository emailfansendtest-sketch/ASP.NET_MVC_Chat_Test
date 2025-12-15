using Application.DTO;
using Application.Exceptions;
using DomainModels;

namespace Application.Implementations.Utilities
{
    internal static class EntitiesMappingExtensions
    {
        /// <summary>
        /// Converts the DTO object into the new <see cref="ChatMessage"/> that is ready to be saved.
        /// </summary>
        /// <param name="dto">The <see cref="ChatMessageDto"/> object that carries new message's data.</param>
        /// <returns></returns>
        public static ChatMessage ToDomain( this ChatMessageDto dto )
            => new ChatMessage
            {
                Content = dto.Content,
                CreatedTime = dto.CreatedTime,
                AuthorId = dto.AuthorId,
                Author = null // Remains null on purpose - the authors data is in the database already, cannot be modified along with sending new message.
            };

        /// <summary>
        /// Converts the domain entity into the DTO message entity.
        /// </summary>
        /// <param name="domainEntity">The original domain entity.</param>
        /// <returns></returns>
        public static ChatMessageDto ToDto( this ChatMessage domainEntity )
        {
            var author = domainEntity.Author;

            if(author == null)
            {
                throw new UserNotFoundException();
            }

            if(string.IsNullOrEmpty( author!.UserName ))
            {
                throw new InvalidUserException();
            }

            return new ChatMessageDto
            {
                Content = domainEntity.Content,
                CreatedTime = domainEntity.CreatedTime,
                AuthorName = author!.UserName,
                AuthorId = domainEntity.AuthorId
            };
        }
    }
}
