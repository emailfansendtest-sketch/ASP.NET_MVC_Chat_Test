using Application.Contracts;
using Application.Interfaces.Utilities;
using Storage.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Implementations.Utilities
{
    internal class MessageBatchWriterService : IMessageBatchWriterService
    {
        private ConcurrentBag<MessageDto> _mainBuffer = new();
        private ConcurrentBag<MessageDto> _spareBuffer = new();

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
        public void Append( MessageDto message )
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

        private async Task SaveMessagesAsync( IEnumerable<MessageDto> messages )
        {
            var newMessages = messages.Select( m => m.ToDatabaseEntity() ).ToArray();

            await _dbService.SaveChangesAsync( newMessages );
        }

    }
}
