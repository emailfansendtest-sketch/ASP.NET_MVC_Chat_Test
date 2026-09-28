using Application.DTO;
using Application.Implementations.Streaming;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;
using Application.Interfaces.Utilities;
using Contracts.Interfaces;
using Contracts.Options;
using DomainModels;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.Streaming;

[TestFixture]
public class MessageStreamServiceTests
{
    [Test]
    public async Task StreamForClientAsync_ReplaysMessagesFromConfiguredWindow()
    {
        var now = new DateTime(
            2026, 9, 28, 12, 0, 0,
            DateTimeKind.Utc );

        var db = new Mock<IDbService>();
        var bus = new Mock<IChatEventBus>();
        var clock = new Mock<IClock>();
        var writer = new Mock<IMessageStreamWriter>();

        clock
            .SetupGet( x => x.UtcNow )
            .Returns( now );

        var entity = new ChatMessage
        {
            AuthorId = "user-1",
            Content = "Old message",
            CreatedTime = now.AddSeconds( -5 ),
            Author = new ChatUser
            {
                Id = "user-1",
                UserName = "Custom_Author"
            }
        };

        db
            .Setup( x => x.GetMessages(
                now.AddSeconds( -30 ),
                now ) )
            .ReturnsAsync( [ entity ] );

        using var cts = new CancellationTokenSource();

        writer
            .Setup( x => x.WriteKeepAliveAsync(
                It.IsAny<CancellationToken>() ) )
            .Callback( cts.Cancel )
            .Returns( Task.CompletedTask );

        var sut = CreateSut(
            bus,
            db,
            clock,
            replayLookbackSeconds: 30 );

        await sut.StreamForClientAsync(
            writer.Object,
            cts.Token );

        db.Verify(
            x => x.GetMessages(
                now.AddSeconds( -30 ),
                now ),
            Times.Once );

        writer.Verify(
            x => x.WriteMessageAsync(
                It.Is<ChatMessageDto>(
                    m =>
                        m.Content == "Old message" &&
                        m.AuthorId == "user-1" &&
                        m.AuthorName == "Custom_Author" ),
                It.IsAny<CancellationToken>() ),
            Times.Once );
    }

    [Test]
    public async Task StreamForClientAsync_SubscribesAndAlwaysUnsubscribes()
    {
        var db = new Mock<IDbService>();
        var bus = new Mock<IChatEventBus>();
        var clock = new Mock<IClock>();
        var writer = new Mock<IMessageStreamWriter>();

        var now = DateTime.UtcNow;

        clock.SetupGet( x => x.UtcNow ).Returns( now );

        db
            .Setup( x => x.GetMessages(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>() ) )
            .ReturnsAsync( Array.Empty<ChatMessage>() );

        using var cts = new CancellationTokenSource();

        writer
            .Setup( x => x.WriteKeepAliveAsync(
                It.IsAny<CancellationToken>() ) )
            .Callback( cts.Cancel )
            .Returns( Task.CompletedTask );

        var sut = CreateSut( bus, db, clock );

        await sut.StreamForClientAsync(
            writer.Object,
            cts.Token );

        bus.Verify(
            x => x.Subscribe( writer.Object ),
            Times.Once );

        bus.Verify(
            x => x.Unsubscribe( writer.Object ),
            Times.Once );
    }

    [Test]
    public void StreamForClientAsync_ReplayFails_StillUnsubscribes()
    {
        var db = new Mock<IDbService>();
        var bus = new Mock<IChatEventBus>();
        var clock = new Mock<IClock>();
        var writer = new Mock<IMessageStreamWriter>();

        clock
            .SetupGet( x => x.UtcNow )
            .Returns( DateTime.UtcNow );

        db
            .Setup( x => x.GetMessages(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>() ) )
            .ThrowsAsync( new InvalidOperationException() );

        var sut = CreateSut( bus, db, clock );

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.StreamForClientAsync(
                writer.Object,
                CancellationToken.None ) );

        bus.Verify(
            x => x.Unsubscribe( writer.Object ),
            Times.Once );
    }

    private static MessageStreamService CreateSut(
        Mock<IChatEventBus> bus,
        Mock<IDbService> db,
        Mock<IClock> clock,
        int replayLookbackSeconds = 30 )
    {
        return new MessageStreamService(
            bus.Object,
            db.Object,
            clock.Object,
            Options.Create(
                new MessageStreamOptions
                {
                    ReplayLookbackS = replayLookbackSeconds,
                    KeepAliveIntervalMs = 60_000
                } ),
            NullLogger<MessageStreamService>.Instance );
    }
}