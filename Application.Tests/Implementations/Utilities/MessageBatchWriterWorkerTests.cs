using Application.Implementations.Utilities;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.Utilities;

[TestFixture]
public class MessageBatchWriterWorkerTests
{
    [Test]
    public async Task ExecuteAsync_WaitsForSecretsBeforeFlushing()
    {
        using var cts =
            new CancellationTokenSource(
                TimeSpan.FromSeconds( 2 ) );

        var writer = new Mock<IMessageWriterService>();
        var readiness = new Mock<ISecretsReadinessTracker>();

        var sequence = new MockSequence();

        readiness
            .InSequence( sequence )
            .Setup( x => x.WaitUntilReadyAsync( cts.Token ) )
            .Returns( Task.CompletedTask );

        writer
            .InSequence( sequence )
            .Setup( x => x.ProcessPendingMessagesAsync() )
            .Callback( cts.Cancel )
            .Returns( Task.CompletedTask );

        var sut = new TestableMessageBatchWriterWorker(
            NullLogger<MessageBatchWriterWorker>.Instance,
            writer.Object,
            readiness.Object,
            Options.Create(
                new PersistenceOptions
                {
                    FlushIntervalMs = 10
                } ) );

        await sut.RunAsync( cts.Token );

        readiness.Verify(
            x => x.WaitUntilReadyAsync(
                It.IsAny<CancellationToken>() ),
            Times.Once );

        writer.Verify(
            x => x.ProcessPendingMessagesAsync(),
            Times.Once );
    }

    private sealed class TestableMessageBatchWriterWorker
        : MessageBatchWriterWorker
    {
        public TestableMessageBatchWriterWorker(
            Microsoft.Extensions.Logging.ILogger<MessageBatchWriterWorker> logger,
            IMessageWriterService writer,
            ISecretsReadinessTracker tracker,
            IOptions<PersistenceOptions> options )
            : base( logger, writer, tracker, options )
        {
        }

        public Task RunAsync( CancellationToken token )
            => ExecuteAsync( token );
    }
}