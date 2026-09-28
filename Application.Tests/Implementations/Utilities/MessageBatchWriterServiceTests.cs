using Application.DTO;
using Application.Implementations.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using DomainModels;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.Utilities;

[TestFixture]
public class MessageBatchWriterServiceTests
{
    [Test]
    public async Task FlushAsync_NoMessages_DoesNotAccessDatabase()
    {
        var db = new Mock<IDbService>();

        using var sut = CreateSut(
            db,
            batchSize: 2 );

        await sut.ProcessPendingMessagesAsync();

        db.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<IEnumerable<ChatMessage>>() ),
            Times.Never );
    }

    [Test]
    public async Task FlushAsync_SplitsMessagesIntoConfiguredBatchSize()
    {
        var savedBatches =
            new List<ChatMessage[]>();

        var db = new Mock<IDbService>();

        db
            .Setup( x => x.SaveChangesAsync(
                It.IsAny<IEnumerable<ChatMessage>>() ) )
            .Callback<IEnumerable<ChatMessage>>(
                messages =>
                    savedBatches.Add( messages.ToArray() ) )
            .Returns( Task.CompletedTask );

        using var sut = CreateSut(
            db,
            batchSize: 2 );

        for(var i = 1; i <= 5; i++)
        {
            await sut.AppendAsync(
                CreateMessage( i ),
                CancellationToken.None );
        }

        await sut.ProcessPendingMessagesAsync();

        Assert.That( savedBatches, Has.Count.EqualTo( 3 ) );

        Assert.That(
            savedBatches[ 0 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "1", "2" } ) );

        Assert.That(
            savedBatches[ 1 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "3", "4" } ) );

        Assert.That(
            savedBatches[ 2 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "5" } ) );
    }

    [Test]
    public async Task FlushAsync_FailedBatch_IsRetriedOnNextFlush()
    {
        var calls = new List<ChatMessage[]>();
        var attempt = 0;

        var db = new Mock<IDbService>();

        db
            .Setup( x => x.SaveChangesAsync(
                It.IsAny<IEnumerable<ChatMessage>>() ) )
            .Returns<IEnumerable<ChatMessage>>(
                messages =>
                {
                    calls.Add( messages.ToArray() );

                    if(++attempt == 1)
                    {
                        throw new InvalidOperationException(
                            "Database unavailable" );
                    }

                    return Task.CompletedTask;
                } );

        using var sut = CreateSut(
            db,
            batchSize: 2 );

        await sut.AppendAsync(
            CreateMessage( 1 ),
            CancellationToken.None );

        await sut.AppendAsync(
            CreateMessage( 2 ),
            CancellationToken.None );

        await Assert.ThrowsAsync<InvalidOperationException>(
            sut.ProcessPendingMessagesAsync );

        await sut.ProcessPendingMessagesAsync();

        Assert.That( calls, Has.Count.EqualTo( 2 ) );

        Assert.That(
            calls[ 0 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "1", "2" } ) );

        Assert.That(
            calls[ 1 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "1", "2" } ) );
    }

    [Test]
    public async Task FlushAsync_FailedBatch_IsRetriedBeforeNewMessages()
    {
        var calls = new List<ChatMessage[]>();
        var attempt = 0;

        var db = new Mock<IDbService>();

        db
            .Setup( x => x.SaveChangesAsync(
                It.IsAny<IEnumerable<ChatMessage>>() ) )
            .Returns<IEnumerable<ChatMessage>>(
                messages =>
                {
                    calls.Add( messages.ToArray() );

                    if(++attempt == 1)
                    {
                        throw new InvalidOperationException();
                    }

                    return Task.CompletedTask;
                } );

        using var sut = CreateSut(
            db,
            batchSize: 2 );

        await sut.AppendAsync(
            CreateMessage( 1 ),
            CancellationToken.None );

        await sut.AppendAsync(
            CreateMessage( 2 ),
            CancellationToken.None );

        await Assert.ThrowsAsync<InvalidOperationException>(
            sut.ProcessPendingMessagesAsync );

        // Arrives after the failed batch.
        await sut.AppendAsync(
            CreateMessage( 3 ),
            CancellationToken.None );

        await sut.ProcessPendingMessagesAsync();

        Assert.That( calls, Has.Count.EqualTo( 3 ) );

        Assert.That(
            calls[ 1 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "1", "2" } ) );

        Assert.That(
            calls[ 2 ].Select( x => x.Content ),
            Is.EqualTo( new[] { "3" } ) );
    }

    [Test]
    public async Task FlushAsync_SuccessfulPartialBatch_IsNotWrittenAgain()
    {
        var db = new Mock<IDbService>();

        using var sut = CreateSut(
            db,
            batchSize: 10 );

        await sut.AppendAsync(
            CreateMessage( 1 ),
            CancellationToken.None );

        await sut.ProcessPendingMessagesAsync();

        // Empty second flush.
        await sut.ProcessPendingMessagesAsync();

        db.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<IEnumerable<ChatMessage>>() ),
            Times.Once );
    }

    private static MessageBatchWriterService CreateSut(
        Mock<IDbService> db,
        int batchSize )
    {
        return new MessageBatchWriterService(
            db.Object,
            Options.Create(
                new MessageWriterOptions
                {
                    MessageBatchSize = batchSize,
                    MessageQueueCapacity = 100
                } ) );
    }

    private static ChatMessageDto CreateMessage( int number )
    {
        return new ChatMessageDto
        {
            Content = number.ToString(),
            AuthorId = "user-1",
            AuthorName = "Custom_Author",
            CreatedTime = DateTime.UtcNow
        };
    }
}