using DomainModels;

namespace Application.Interfaces.Sending
{
    /// <summary>
    /// Used for the <see cref="ChatMessage"/> entities creation before sending.
    /// </summary>
    internal interface IChatMessageFactory
    {
        /// <summary>
        /// Creates the new <see cref="ChatMessage"/> using the argument as the message content, 
        /// currently authenticated user as the author and current time as the creation time.
        /// </summary>
        /// <param name="content">The content of the new message.</param>
        /// <returns></returns>
        Task<ChatMessage> CreateAsync( string content );
    }
}
