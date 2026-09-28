using Application.DTO;
using Application.Exceptions;
using Application.Implementations.Utilities;
using DomainModels;
using NUnit.Framework;

namespace Application.Tests.Implementations.Utilities;

[TestFixture]
public class EntitiesMappingExtensionsTests
{
    [Test]
    public void ToDomain_MapsPersistedFields()
    {
        var time = DateTime.UtcNow;

        var dto = new ChatMessageDto
        {
            AuthorId = "user-1",
            AuthorName = "Custom_Author",
            Content = "Message",
            CreatedTime = time
        };

        var result = dto.ToDomain();

        Assert.Multiple( () =>
        {
            Assert.That( result.AuthorId, Is.EqualTo( "user-1" ) );
            Assert.That( result.Content, Is.EqualTo( "Message" ) );
            Assert.That( result.CreatedTime, Is.EqualTo( time ) );

            // Deliberately not attached to avoid updating
            // the existing Identity entity.
            Assert.That( result.Author, Is.Null );
        } );
    }

    [Test]
    public void ToDto_MapsExpectedFields()
    {
        var time = DateTime.UtcNow;

        var entity = new ChatMessage
        {
            AuthorId = "user-1",
            Author = new ChatUser
            {
                Id = "user-1",
                UserName = "Custom_Author"
            },
            Content = "Message",
            CreatedTime = time
        };

        var result = entity.ToDto();

        Assert.Multiple( () =>
        {
            Assert.That( result.AuthorId, Is.EqualTo( "user-1" ) );
            Assert.That( result.AuthorName, Is.EqualTo( "Custom_Author" ) );
            Assert.That( result.Content, Is.EqualTo( "Message" ) );
            Assert.That( result.CreatedTime, Is.EqualTo( time ) );
        } );
    }

    [Test]
    public void ToDto_AuthorIsNull_ThrowsUserNotFoundException()
    {
        var entity = new ChatMessage
        {
            AuthorId = "user-1",
            Author = null
        };

        Assert.Throws<UserNotFoundException>(
            () => entity.ToDto() );
    }

    [TestCase( null )]
    [TestCase( "" )]
    public void ToDto_AuthorHasInvalidName_ThrowsInvalidUserException(
        string? userName )
    {
        var entity = new ChatMessage
        {
            Author = new ChatUser
            {
                UserName = userName
            }
        };

        Assert.Throws<InvalidUserException>(
            () => entity.ToDto() );
    }
}