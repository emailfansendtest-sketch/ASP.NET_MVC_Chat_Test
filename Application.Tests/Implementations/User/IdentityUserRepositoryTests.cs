using System.Security.Claims;
using Application.Implementations.User;
using DomainModels;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.User;

[TestFixture]
public class IdentityUserRepositoryTests
{
    [Test]
    public async Task GetByPrincipal_ReturnsUserFromUserManager()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "user-1")
                ] ) );

        var expected = new ChatUser
        {
            Id = "user-1",
            UserName = "Custom_Author"
        };

        var userManager = CreateUserManager();

        userManager
            .Setup( x => x.GetUserAsync( principal ) )
            .ReturnsAsync( expected );

        var sut =
            new IdentityUserRepository(
                userManager.Object );

        var result =
            await sut.GetByPrincipal( principal );

        Assert.That( result, Is.SameAs( expected ) );
    }

    private static Mock<UserManager<ChatUser>>
        CreateUserManager()
    {
        var store =
            new Mock<IUserStore<ChatUser>>();

        return new Mock<UserManager<ChatUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null! );
    }
}