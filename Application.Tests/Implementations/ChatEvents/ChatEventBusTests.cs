using Application.DTO;
using Application.Implementations.ChatEvents;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.Streaming;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.ChatEvents;

[TestFixture]
public class ChatEventBusTests
{
    [Test]
    public void Subscribe_NewListener_CreatesSubscriber()
    {
        var listener = new Mock<IMessageStreamWriter>();
        var subscriber = new Mock<IChatEventSubscriber>();
        var factory = new Mock<IChatEventSubscriberFactory>();

        factory
            .Setup( x => x.Create( listener.Object ) )
            .Returns( subscriber.Object );

        var sut = new ChatEventBus( factory.Object );

        sut.Subscribe( listener.Object );

        factory.Verify(
            x => x.Create( listener.Object ),
            Times.Once );
    }

    [Test]
    public void Subscribe_SameListenerTwice_DoesNotCreateSecondSubscriber()
    {
        var listener = new Mock<IMessageStreamWriter>();
        var subscriber = new Mock<IChatEventSubscriber>();
        var factory = new Mock<IChatEventSubscriberFactory>();

        factory
            .Setup( x => x.Create( listener.Object ) )
            .Returns( subscriber.Object );

        var sut = new ChatEventBus( factory.Object );

        sut.Subscribe( listener.Object );
        sut.Subscribe( listener.Object );

        factory.Verify(
            x => x.Create( listener.Object ),
            Times.Once );
    }

    [Test]
    public async Task PublishAsync_ForwardsMessageToAllSubscribers()
    {
        var listener1 = new Mock<IMessageStreamWriter>();
        var listener2 = new Mock<IMessageStreamWriter>();

        var subscriber1 = new Mock<IChatEventSubscriber>();
        var subscriber2 = new Mock<IChatEventSubscriber>();

        var factory = new Mock<IChatEventSubscriberFactory>();

        factory
            .Setup( x => x.Create( listener1.Object ) )
            .Returns( subscriber1.Object );

        factory
            .Setup( x => x.Create( listener2.Object ) )
            .Returns( subscriber2.Object );

        var sut = new ChatEventBus( factory.Object );

        sut.Subscribe( listener1.Object );
        sut.Subscribe( listener2.Object );

        var message = new ChatMessageDto();

        await sut.PublishAsync( message );

        subscriber1.Verify(
            x => x.TryWrite( message ),
            Times.Once );

        subscriber2.Verify(
            x => x.TryWrite( message ),
            Times.Once );
    }

    [Test]
    public void Unsubscribe_SubscribedListener_DisposesSubscriber()
    {
        var listener = new Mock<IMessageStreamWriter>();
        var subscriber = new Mock<IChatEventSubscriber>();
        var factory = new Mock<IChatEventSubscriberFactory>();

        factory
            .Setup( x => x.Create( listener.Object ) )
            .Returns( subscriber.Object );

        var sut = new ChatEventBus( factory.Object );

        sut.Subscribe( listener.Object );
        sut.Unsubscribe( listener.Object );

        subscriber.Verify(
            x => x.Dispose(),
            Times.Once );
    }

    [Test]
    public async Task Unsubscribe_RemovesSubscriberFromPublication()
    {
        var listener = new Mock<IMessageStreamWriter>();
        var subscriber = new Mock<IChatEventSubscriber>();
        var factory = new Mock<IChatEventSubscriberFactory>();

        factory
            .Setup( x => x.Create( listener.Object ) )
            .Returns( subscriber.Object );

        var sut = new ChatEventBus( factory.Object );

        sut.Subscribe( listener.Object );
        sut.Unsubscribe( listener.Object );

        await sut.PublishAsync( new ChatMessageDto() );

        subscriber.Verify(
            x => x.TryWrite( It.IsAny<ChatMessageDto>() ),
            Times.Never );
    }
}