using DomainModels;

namespace Application.Interfaces.Utilities
{
    /// <summary>
    /// Buffers chat messages in memory and periodically flushes them to durable storage.
    /// It is expected to be thread-safe.
    /// </summary>
    internal interface IMessageWriterService : IDisposable
    {
        /// <summary>
        /// Appends a new message to the in-memory buffer for later persistence.
        /// </summary>
        /// <param name="messageContract">The message to buffer.</param>
        /// <param name="cancellationToken">Indicates that the message will not be saved due to some server problem.</param>
        Task AppendAsync( ChatMessage messageContract, CancellationToken cancellationToken );

        /// <summary>
        /// Persists buffered messages and clears the buffer.
        /// Implementations should be no-op if the buffer is empty.
        /// </summary>
        Task FlushAsync( );
    }
}
