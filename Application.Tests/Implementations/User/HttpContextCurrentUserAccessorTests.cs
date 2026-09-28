using System.Security.Claims;
using Application.Implementations.User;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace Application.Tests.Implementations.User;

[TestFixture]
public class HttpContextCurrentUserAccessorTests
{
    [Test]
    public void Principal_HttpContextExists_ReturnsItsUser()
    {
        var expected = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "Custom_Author")
                ],
                "Test" ) );

        var context = new DefaultHttpContext
        {
            User = expected
        };

        var accessor = new Mock<IHttpContextAccessor>();

        accessor
            .SetupGet( x => x.HttpContext )
            .Returns( context );

        var sut = new HttpContextCurrentUserAccessor(
            accessor.Object );

        Assert.That( sut.Principal, Is.SameAs( expected ) );
    }

    [Test]
    public void Principal_NoHttpContext_ReturnsEmptyPrincipal()
    {
        var accessor = new Mock<IHttpContextAccessor>();

        accessor
            .SetupGet( x => x.HttpContext )
            .Returns( (HttpContext?)null );

        var sut = new HttpContextCurrentUserAccessor(
            accessor.Object );

        Assert.That( sut.Principal, Is.Not.Null );
        Assert.That( sut.Principal.Identity, Is.Null );
    }
}