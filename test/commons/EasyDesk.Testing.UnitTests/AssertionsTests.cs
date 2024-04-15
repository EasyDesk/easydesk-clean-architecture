using NSubstitute;
using Shouldly;

namespace EasyDesk.Testing.UnitTests;

public class AssertionsTests
{
    [Fact]
    public Task VerifyTest()
    {
        return Verify(new
        {
            Id = 42,
        });
    }

    [Fact]
    public void ShouldlyTest()
    {
        true.ShouldBeTrue();
    }

    [Fact]
    public void NSubstituteTest()
    {
        var sut = Substitute.For<Func<int>>();
        sut().ReturnsForAnyArgs(7);
        Assert.StrictEqual(7, sut());
    }
}
