using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;
using Contracts.Options;
using DomainModels;

using Microsoft.Extensions.Logging;
using System.Threading.Channels;


namespace Application.Implementations.ChatEvents
{
    internal class ChatEventSubscriber : IChatEventSubscriber
    {
        private readonly IMessageStreamWriter _listener;
        private readonly ILogger<ChatEventSubscriber> _logger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Channel<ChatMessage> _channel;
        private readonly Task _pump;

        public ChatEventSubscriber( 
            ChatEventOptions options, 
            IMessageStreamWriter listener, 
            ILoggerFactory loggerFactory )
        {
            _listener = listener;
            _logger = loggerFactory.CreateLogger<ChatEventSubscriber>();

            _channel = Channel.CreateBounded<ChatMessage>( 
                new BoundedChannelOptions( options.SubscriberCapacity )
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest // or DropNewest / Wait
            } );

            _pump = PumpAsync();
        }

        public void TryWrite( ChatMessage message )
        {
            if( !_channel.Writer.TryWrite( message ) )
            {
                _logger.LogDebug( "Failed writing the message." );
            }
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
            _cts.Cancel();
            _pump.ContinueWith( t =>
            {
                if(t.Exception != null)
                    _logger.LogDebug( t.Exception, "Subscriber pump ended with error." );
            }, TaskScheduler.Default );
            _cts.Dispose();
        }

        private async Task PumpAsync( )
        {
            var reader = _channel.Reader;
            var ct = _cts.Token;
            try
            {
                await foreach(var msg in reader.ReadAllAsync( ct ).ConfigureAwait( false ))
                {
                    await _listener.WriteMessageAsync( msg, ct ).ConfigureAwait( false );
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception ex)
            {
                _logger.LogDebug( ex, "Subscriber pump failed for {Listener}.", nameof( _listener ) );
            }
            // TODO
            //finally
            //{
            //    _subs.TryRemove( _listener, out _ );
            //}
        }
    }
}
