using DomainModels;

namespace Application.Interfaces.Utilities
{
    /// <summary>
    /// Buffers chat messages in memory and periodically flushes them to durable storage.
    /// It is expected to be thread-safe.
    /// </summary>
    internal interface IMessageBatchWriterService
    {
        /// <summary>
        /// Appends a new message to the in-memory buffer for later persistence.
        /// </summary>
        /// <param name="messageContract">The message to buffer.</param>
        void Append( ChatMessage messageContract );

        /// <summary>
        /// Persists buffered messages and clears the buffer.
        /// Implementations should be no-op if the buffer is empty.
        /// </summary>
        Task FlushAsync();
    }
}
