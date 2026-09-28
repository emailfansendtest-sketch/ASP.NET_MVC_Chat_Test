using Application.Implementations.Utilities;
using NUnit.Framework;

namespace Application.Tests.Implementations.Utilities;

[TestFixture]
public class SystemClockTests
{
    [Test]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        var before = DateTime.UtcNow;

        var sut = new SystemClock();

        var result = sut.UtcNow;

        var after = DateTime.UtcNow;

        Assert.That( result, Is.InRange( before, after ) );
        Assert.That( result.Kind, Is.EqualTo( DateTimeKind.Utc ) );
    }
}