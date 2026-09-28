using Application.DTO;
using Application.Implementations.Sending;
using Application.Interfaces.ChatEvents;
using Application.Interfaces.EntityCreation;
using Application.Interfaces.Utilities;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.Sending;

[TestFixture]
public class MessageSenderServiceTests
{
    [Test]
    public async Task SendAsync_CreatesBuffersAndPublishesMessage()
    {
        var message = new ChatMessageDto
        {
            Content = "Hello",
            AuthorId = "1",
            AuthorName = "Custom_Author"
        };

        var factory = new Mock<IChatMessageDtoFactory>();
        var writer = new Mock<IMessageWriterService>();
        var eventBus = new Mock<IChatEventBus>();

        factory
            .Setup( x => x.CreateAsync( "Hello" ) )
            .ReturnsAsync( message );

        var sut = new MessageSenderService(
            eventBus.Object,
            writer.Object,
            factory.Object );

        using var cts = new CancellationTokenSource();

        await sut.SendAsync( "Hello", cts.Token );

        factory.Verify(
            x => x.CreateAsync( "Hello" ),
            Times.Once );

        writer.Verify(
            x => x.AppendAsync( message, cts.Token ),
            Times.Once );

        eventBus.Verify(
            x => x.PublishAsync( message ),
            Times.Once );
    }

    [Test]
    public void SendAsync_BufferingFails_DoesNotPublishMessage()
    {
        var message = new ChatMessageDto
        {
            Content = "Hello"
        };

        var factory = new Mock<IChatMessageDtoFactory>();
        var writer = new Mock<IMessageWriterService>();
        var eventBus = new Mock<IChatEventBus>();

        factory
            .Setup( x => x.CreateAsync( It.IsAny<string>() ) )
            .ReturnsAsync( message );

        writer
            .Setup( x => x.AppendAsync(
                message,
                It.IsAny<CancellationToken>() ) )
            .ThrowsAsync( new InvalidOperationException() );

        var sut = new MessageSenderService(
            eventBus.Object,
            writer.Object,
            factory.Object );

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SendAsync( "Hello" ) );

        eventBus.Verify(
            x => x.PublishAsync( It.IsAny<ChatMessageDto>() ),
            Times.Never );
    }

    [Test]
    public void SendAsync_MessageCreationFails_DoesNotBufferOrPublish()
    {
        var factory = new Mock<IChatMessageDtoFactory>();
        var writer = new Mock<IMessageWriterService>();
        var eventBus = new Mock<IChatEventBus>();

        factory
            .Setup( x => x.CreateAsync( It.IsAny<string>() ) )
            .ThrowsAsync( new InvalidOperationException() );

        var sut = new MessageSenderService(
            eventBus.Object,
            writer.Object,
            factory.Object );

        Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.SendAsync( "Hello" ) );

        writer.Verify(
            x => x.AppendAsync(
                It.IsAny<ChatMessageDto>(),
                It.IsAny<CancellationToken>() ),
            Times.Never );

        eventBus.Verify(
            x => x.PublishAsync( It.IsAny<ChatMessageDto>() ),
            Times.Never );
    }
}