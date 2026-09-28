using System.Security.Claims;
using Application.Implementations.User;
using Application.Interfaces.User;
using DomainModels;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.User;

[TestFixture]
public class UserServiceTests
{
    [Test]
    public async Task GetCurrentUserAsync_UsesCurrentPrincipal()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user-1")
                },
                "Test" ) );

        var expectedUser = new ChatUser
        {
            Id = "user-1",
            UserName = "Custom_Author"
        };

        var accessor = new Mock<ICurrentUserAccessor>();
        var repository = new Mock<IUserRepository>();

        accessor
            .SetupGet( x => x.Principal )
            .Returns( principal );

        repository
            .Setup( x => x.GetByPrincipal( principal ) )
            .ReturnsAsync( expectedUser );

        var sut = new UserService(
            accessor.Object,
            repository.Object );

        var result = await sut.GetCurrentUserAsync();

        Assert.That( result, Is.SameAs( expectedUser ) );

        repository.Verify(
            x => x.GetByPrincipal( principal ),
            Times.Once );
    }

    [Test]
    public async Task GetCurrentUserAsync_UserDoesNotExist_ReturnsNull()
    {
        var principal = new ClaimsPrincipal();

        var accessor = new Mock<ICurrentUserAccessor>();
        var repository = new Mock<IUserRepository>();

        accessor
            .SetupGet( x => x.Principal )
            .Returns( principal );

        repository
            .Setup( x => x.GetByPrincipal( principal ) )
            .ReturnsAsync( (ChatUser?)null );

        var sut = new UserService(
            accessor.Object,
            repository.Object );

        var result = await sut.GetCurrentUserAsync();

        Assert.That( result, Is.Null );
    }
}