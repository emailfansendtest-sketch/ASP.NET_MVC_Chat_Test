using DomainModels;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;

using System.Collections.Concurrent;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterService : IMessageBatchWriterService
    {
        private ConcurrentBag<ChatMessage> _mainBuffer = new();
        private ConcurrentBag<ChatMessage> _spareBuffer = new();

        private readonly ReaderWriterLockSlim _lock = new();
        private readonly IDbService _dbService;

        private int _flushProgressIndicator;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="dbService">The database service.</param>
        public MessageBatchWriterService( IDbService dbService )
        {
            _dbService = dbService;
        }

        /// <inheritdoc />
        public void Append( ChatMessage message )
        {
            bool readerLockTaken = false;

            try
            {
                _lock.EnterReadLock();
                readerLockTaken = true;
                _mainBuffer.Add( message );
            }
            finally
            {
                if(readerLockTaken)
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public async Task FlushAsync()
        {
            var isFlushHappening = Interlocked.CompareExchange( ref _flushProgressIndicator, 1, 0 );

            if(isFlushHappening == 1)
            {
                return; // The previous flush is in progress, so the current one is postponed
            }
            bool writerLockTaken = false;

            try
            {
                _lock.EnterWriteLock();
                writerLockTaken = true;

                (_spareBuffer, _mainBuffer) = (_mainBuffer, _spareBuffer);
            }
            finally
            {
                if(writerLockTaken)
                {
                    _lock.ExitWriteLock();
                }
            }

            await SaveMessagesAsync( _spareBuffer );
            _spareBuffer.Clear();
            Interlocked.Exchange( ref _flushProgressIndicator, 0 );
        }

        private async Task SaveMessagesAsync( IEnumerable<ChatMessage> messages )
        {
            await _dbService.SaveChangesAsync( messages.ToArray() );
        }

    }
}
