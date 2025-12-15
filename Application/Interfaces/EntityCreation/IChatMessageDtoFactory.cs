using Application.DTO;

namespace Application.Interfaces.EntityCreation
{
    /// <summary>
    /// Creates <see cref="ChatMessageDto"/> instances from raw message content,
    /// enriching them with current user data and a UTC timestamp.
    /// </summary>
    internal interface IChatMessageDtoFactory
    {
        /// <summary>
        /// Creates a new <see cref="ChatMessageDto"/> using the supplied content, the currently authenticated user
        /// as the author, and the current UTC time as the creation time.
        /// </summary>
        /// <param name="content">Message content submitted by the client.</param>
        /// <returns>A fully populated <see cref="ChatMessageDto"/>.</returns>
        Task<ChatMessageDto> CreateAsync( string content );
    }
}
