using System.Collections.Concurrent;
using Application.DTO;
using Application.Implementations.ChatEvents;
using Application.Interfaces.Streaming;
using Contracts.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.ChatEvents;

[TestFixture]
public class ChatEventSubscriberTests
{
    [Test]
    public async Task TryWrite_ForwardsMessageToListener()
    {
        var writer = new Mock<IMessageStreamWriter>();

        var delivered =
            new TaskCompletionSource<ChatMessageDto>(
                TaskCreationOptions.RunContinuationsAsynchronously );

        writer
            .Setup( x => x.WriteMessageAsync(
                It.IsAny<ChatMessageDto>(),
                It.IsAny<CancellationToken>() ) )
            .Returns(
                ( ChatMessageDto message, CancellationToken _ ) =>
                {
                    delivered.TrySetResult( message );
                    return Task.CompletedTask;
                } );

        using var sut = new ChatEventSubscriber(
            new ChatEventOptions
            {
                SubscriberCapacity = 10
            },
            writer.Object,
            NullLoggerFactory.Instance );

        var expected = new ChatMessageDto
        {
            Content = "Hello"
        };

        sut.TryWrite( expected );

        var actual = await delivered.Task.WaitAsync(
            TimeSpan.FromSeconds( 1 ) );

        Assert.That( actual, Is.SameAs( expected ) );
    }

    [Test]
    public async Task FullBuffer_DropsOldestWaitingMessage()
    {
        var writer = new Mock<IMessageStreamWriter>();

        var firstWriteStarted =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously );

        var allowFirstWriteToFinish =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously );

        var received =
            new ConcurrentQueue<ChatMessageDto>();

        var call = 0;

        writer
            .Setup( x => x.WriteMessageAsync(
                It.IsAny<ChatMessageDto>(),
                It.IsAny<CancellationToken>() ) )
            .Returns(
                async ( ChatMessageDto message, CancellationToken _ ) =>
                {
                    received.Enqueue( message );

                    if(Interlocked.Increment( ref call ) == 1)
                    {
                        firstWriteStarted.TrySetResult();

                        await allowFirstWriteToFinish.Task;
                    }
                } );

        using var sut = new ChatEventSubscriber(
            new ChatEventOptions
            {
                SubscriberCapacity = 1
            },
            writer.Object,
            NullLoggerFactory.Instance );

        var first = new ChatMessageDto { Content = "1" };
        var second = new ChatMessageDto { Content = "2" };
        var third = new ChatMessageDto { Content = "3" };

        sut.TryWrite( first );

        await firstWriteStarted.Task.WaitAsync(
            TimeSpan.FromSeconds( 1 ) );

        // Pump is occupied writing #1.
        // Capacity is only one.
        sut.TryWrite( second );
        sut.TryWrite( third );

        // #3 replaces #2 because DropOldest is configured.
        allowFirstWriteToFinish.TrySetResult();

        await WaitUntilAsync(
            () => received.Count >= 2 );

        var actual = received.ToArray();

        Assert.That( actual.Select( x => x.Content ),
            Is.EqualTo( ["1", "3"] ) );
    }

    private static async Task WaitUntilAsync(
        Func<bool> condition )
    {
        using var timeout =
            new CancellationTokenSource( TimeSpan.FromSeconds( 1 ) );

        while(!condition())
        {
            await Task.Delay( 10, timeout.Token );
        }
    }
}