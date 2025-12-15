namespace Application.Interfaces.Sending
{
    /// <summary>
    /// Accepts raw message input from the web layer and orchestrates the send pipeline:
    /// creates a message DTO (author + timestamp), publishes it for real-time streaming,
    /// and schedules it for persistence.
    /// </summary>
    public interface IMessageSenderService
    {
        /// <summary>
        /// Creates and processes a new chat message authored by the currently authenticated user.
        /// </summary>
        /// <param name="content">Raw message text submitted by the client.</param>
        /// <param name="ct">Cancellation token for the end-to-end send operation.</param>
        Task SendAsync( string content, CancellationToken ct = default );
    }
}
