using DomainModels;

namespace Application.Contracts
{
    /// <summary>
    /// The MVC model of the message sent by the chat user.
    /// </summary>
    public class MessageDto
    {
        /// <summary>
        /// The text content of the message.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// The creation time of the message.
        /// </summary>
        public required DateTime CreatedTime { get; set; }

        /// <summary>
        /// The user who has sent the message.
        /// </summary>
        public required ChatUser Author { get; set; }
    }
}
