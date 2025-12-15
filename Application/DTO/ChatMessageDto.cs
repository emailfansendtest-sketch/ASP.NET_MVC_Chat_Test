
namespace Application.DTO
{
    /// <summary>
    /// Application-layer data transfer object that represents a chat message as it flows through
    /// sending, broadcasting/streaming, and persistence.
    /// Carries denormalized author data to avoid loading navigation properties at runtime.
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// Message text content as submitted by the author.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Stable identifier of the author (e.g., ASP.NET Identity user id).
        /// </summary>
        public string AuthorId { get; set; } = string.Empty;

        /// <summary>
        /// Display name captured at send time (typically Identity UserName).
        /// </summary>
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the message was created.
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
