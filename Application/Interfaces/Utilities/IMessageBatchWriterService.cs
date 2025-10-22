using Application.Contracts;

namespace Application.Interfaces.Utilities
{
    internal interface IMessageBatchWriterService
    {
        /// <summary>
        /// Append the new message to be flushed.
        /// </summary>
        /// <param name="messageContract">The new message.</param>
        void Append( MessageDto messageContract );

        /// <summary>
        /// Flushes the appended messages into the database.
        /// </summary>
        Task FlushAsync();
    }
}
