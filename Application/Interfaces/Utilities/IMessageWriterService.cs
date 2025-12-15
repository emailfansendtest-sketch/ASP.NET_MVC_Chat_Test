using Application.DTO;

namespace Application.Interfaces.Utilities
{
    /// <summary>
    /// Buffers <see cref="ChatMessageDto"/> instances in memory and periodically flushes them to durable storage.
    /// It is expected to be thread-safe.
    /// </summary>
    internal interface IMessageWriterService : IDisposable
    {
        /// <summary>
        /// Appends a new message to the in-memory buffer for later persistence.
        /// </summary>
        /// <param name="messageDto">DTO that carries message data to buffer.</param>
        /// <param name="cancellationToken">Cancellation token for the enqueue operation.</param>
        Task AppendAsync( ChatMessageDto messageDto, CancellationToken cancellationToken );

        /// <summary>
        /// Persists buffered messages and clears the buffer.
        /// Implementations should be a no-op if the buffer is empty.
        /// </summary>
        Task FlushAsync();
    }
}
