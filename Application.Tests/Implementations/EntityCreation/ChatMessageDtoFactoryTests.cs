using Application.Exceptions;
using Application.Implementations.EntityCreation;
using Application.Interfaces.User;
using Application.Interfaces.Utilities;
using DomainModels;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.EntityCreation;

[TestFixture]
public class ChatMessageDtoFactoryTests
{
    private Mock<IUserService> _userService = null!;
    private Mock<IClock> _clock = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>();
        _clock = new Mock<IClock>();
    }

    [Test]
    public async Task CreateAsync_ValidUser_CreatesExpectedDto()
    {
        var now = new DateTime(
            2026, 9, 28, 10, 30, 0,
            DateTimeKind.Utc );

        var user = new ChatUser
        {
            Id = "user-1",
            UserName = "Custom_Author"
        };

        _userService
            .Setup( x => x.GetCurrentUserAsync() )
            .ReturnsAsync( user );

        _clock
            .SetupGet( x => x.UtcNow )
            .Returns( now );

        var sut = new ChatMessageDtoFactory(
            _userService.Object,
            _clock.Object );

        var result = await sut.CreateAsync( "Hello" );

        Assert.Multiple( () =>
        {
            Assert.That( result.Content, Is.EqualTo( "Hello" ) );
            Assert.That( result.AuthorId, Is.EqualTo( "user-1" ) );
            Assert.That( result.AuthorName, Is.EqualTo( "Custom_Author" ) );
            Assert.That( result.CreatedTime, Is.EqualTo( now ) );
        } );
    }

    [Test]
    public void CreateAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        _userService
            .Setup( x => x.GetCurrentUserAsync() )
            .ReturnsAsync( (ChatUser?)null );

        var sut = new ChatMessageDtoFactory(
            _userService.Object,
            _clock.Object );

        Assert.ThrowsAsync<UserNotFoundException>(
            () => sut.CreateAsync( "Hello" ) );
    }

    [TestCase( null )]
    [TestCase( "" )]
    public void CreateAsync_InvalidUserName_ThrowsInvalidUserException(
        string? userName )
    {
        _userService
            .Setup( x => x.GetCurrentUserAsync() )
            .ReturnsAsync( new ChatUser
            {
                Id = "user-1",
                UserName = userName
            } );

        var sut = new ChatMessageDtoFactory(
            _userService.Object,
            _clock.Object );

        Assert.ThrowsAsync<InvalidUserException>(
            () => sut.CreateAsync( "Hello" ) );
    }
}