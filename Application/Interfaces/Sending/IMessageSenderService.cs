
namespace Application.Interfaces.Sending
{

    /// <summary>
    /// Coordinates intake of a new chat message from the web layer:
    /// validates basic input, enriches it with author metadata, persists via a batch writer,
    /// and publishes it to the streaming pipeline so connected clients receive it.
    /// </summary>
    public interface IMessageSenderService
    {
        /// <summary>
        /// Creates and processes a new chat message authored by the presently authentified user.
        /// </summary>
        /// <param name="content">Raw message text submitted by the client.</param>
        /// <param name="ct">Cancellation token for the end-to-end send operation.</param>
        /// <returns></returns>
        Task SendAsync( string content, CancellationToken ct = default );
    }
}
