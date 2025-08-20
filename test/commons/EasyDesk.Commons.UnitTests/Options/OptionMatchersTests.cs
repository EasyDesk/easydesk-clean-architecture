using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Options;

public class OptionMatchersTests
{
    private const int Value = 5;
    private const int Other = 10;

    private const string Reference = "abc";

    [Fact]
    public void OrElse_ShouldReturnTheGivenValue_IfOptionIsEmpty()
    {
        NoneT<int>().OrElse(Other).ShouldBe(Other);
    }

    [Fact]
    public void OrElse_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        Some(Value).OrElse(Other).ShouldBe(Value);
    }

    [Fact]
    public void OrElseGet_ShouldReturnTheValueReturnedByTheGivenFunction_IfOptionIsEmpty()
    {
        NoneT<int>().OrElseGet(() => Other).ShouldBe(Other);
    }

    [Fact]
    public void OrElseGet_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        Some(Value).OrElseGet(() => Other).ShouldBe(Value);
    }

    [Fact]
    public async Task OrElseGetAsync_ShouldReturnTheGivenTask_IfOptionIsEmpty()
    {
        var result = await NoneT<int>().OrElseGetAsync(() => Task.FromResult(Other));
        result.ShouldBe(Other);
    }

    [Fact]
    public async Task OrElseGetAsync_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        var result = await Some(Value).OrElseGetAsync(() => Task.FromResult(Other));
        result.ShouldBe(Value);
    }

    [Fact]
    public void OrElseDefault_ShouldReturnTheDefaultValueForTheType_IfOptionIsEmpty()
    {
        NoneT<int>().OrElseDefault().ShouldBe(0);
    }

    [Fact]
    public void OrElseDefault_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        Some(Value).OrElseDefault().ShouldBe(Value);
    }

    [Fact]
    public void OrElseNull_ShouldReturnNull_IfOptionIsEmpty()
    {
        NoneT<string>().OrElseNull().ShouldBeNull();
    }

    [Fact]
    public void OrElseNull_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        Some(Reference).OrElseNull().ShouldBe(Reference);
    }

    [Fact]
    public void OrElseNull_ShouldReturnNull_IfOptionIsEmpty_WithNullableStruct()
    {
        NoneT<int?>().OrElse(null).ShouldBeNull();
    }

    [Fact]
    public void OrElseNull_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty_WithNullableStruct()
    {
        Some<int?>(5).OrElse(null).ShouldBe(5);
    }

    [Fact]
    public void OrElseNull_ShouldReturnNull_IfOptionIsEmpty_WithStruct()
    {
        NoneT<int>().OrElseNothing().ShouldBeNull();
    }

    [Fact]
    public void OrElseNull_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty_WithStruct()
    {
        Some(5).OrElseNothing().ShouldBe(5);
    }

    [Fact]
    public void OrElseThrow_ShouldThrowTheGivenException_IfOptionIsEmpty()
    {
        Should.Throw<Exception>(() => NoneT<int>().OrElseThrow(() => new InvalidOperationException()));
    }

    [Fact]
    public void OrElseThrow_ShouldReturnTheValueInsideTheOption_IfOptionIsNotEmpty()
    {
        Some(Value).OrElseThrow(() => new InvalidOperationException()).ShouldBe(Value);
    }

    [Fact]
    public void Conditionally_ShouldReturnTheOriginalValue_IfOptionIsEmpty()
    {
        7.Conditionally(NoneT<string>(), (_, _) => 1).ShouldBe(7);
    }

    [Fact]
    public void Conditionally_ShouldMapTheValue_IfOptionIsNotEmpty()
    {
        7.Conditionally(Some(5), (a, b) => a + b).ShouldBe(12);
    }
}
