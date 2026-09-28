using Application.Implementations.ChatEvents;
using Application.Interfaces.Streaming;
using Contracts.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.ChatEvents;

[TestFixture]
public class ChatEventSubscriberFactoryTests
{
    [Test]
    public void Create_ReturnsSubscriber()
    {
        var options = Options.Create(
            new ChatEventOptions
            {
                SubscriberCapacity = 10
            } );

        var sut = new ChatEventSubscriberFactory(
            NullLoggerFactory.Instance,
            options );

        var writer = new Mock<IMessageStreamWriter>();

        using var result = sut.Create( writer.Object );

        Assert.That( result, Is.Not.Null );
        Assert.That( result, Is.TypeOf<ChatEventSubscriber>() );
    }
}