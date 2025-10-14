using MVC_SSL_Chat.Models;

namespace MVC_SSL_Chat.Internal.Interfaces
{
    /// <summary>
    /// TODO
    /// The interface of the buffering service - the service that accumulates the new messages and flushes them into the database under certain conditions
    /// </summary>
    public interface IBufferService
    {
        /// <summary>
        /// Append the new message to be flushed.
        /// </summary>
        /// <param name="messageModel">The new message.</param>
        void Append( MessageModel messageModel );

        /// <summary>
        /// Flushes the appended messages into the database.
        /// </summary>
        Task FlushAsync();
    }
}
