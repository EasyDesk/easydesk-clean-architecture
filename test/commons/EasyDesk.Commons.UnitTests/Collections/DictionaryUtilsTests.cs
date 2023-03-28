using EasyDesk.Commons.Collections;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Collections;

public class DictionaryUtilsTests
{
    private readonly Dictionary<string, int> _sut = new()
    {
        { "one", 1 },
        { "two", 2 },
        { "three", 3 }
    };

    [Fact]
    public void GetOption_ShouldReturnNone_IfTheKeyIsNotInTheDictionary()
    {
        _sut.GetOption("four").ShouldBeEmpty();
    }

    [Fact]
    public void GetOption_ShouldReturnTheCorrespondingValue_IfTheKeyIsInTheDictionary()
    {
        _sut.GetOption("one").ShouldContain(1);
    }

    [Fact]
    public void Merge_ShouldAddANewKey_IfTheKeyIsNotInTheDictionary()
    {
        var key = "four";
        var value = 4;
        var combiner = Substitute.For<Func<int, int, int>>();

        _sut.Merge(key, value, combiner);

        _sut.ShouldContainKeyAndValue(key, value);
        combiner.DidNotReceiveWithAnyArgs()(default, default);
    }

    [Fact]
    public void Merge_ShouldCombineTheOldValueWithTheNewOne_IfTheKeyIsInTheDictionary()
    {
        var key = "one";
        var value = 4;
        var oldValue = _sut[key];

        _sut.Merge(key, value, (a, b) => a + b);

        _sut.ShouldContainKeyAndValue(key, value + oldValue);
    }

    [Fact]
    public void GetOrAdd_ShouldAddANewKey_IfTheKeyIsNotInTheDictionary()
    {
        var key = "four";
        var value = 4;

        var result = _sut.GetOrAdd(key, () => value);

        result.ShouldBe(value);
        _sut.ShouldContainKeyAndValue(key, value);
    }

    [Fact]
    public void GetOrAdd_ShouldReturnTheCorrespondingValue_IfTheKeyIsInTheDictionary()
    {
        var key = "one";
        var currentValue = _sut[key];
        var supplier = Substitute.For<Func<int>>();

        var result = _sut.GetOrAdd(key, supplier);

        result.ShouldBe(currentValue);
        supplier.DidNotReceiveWithAnyArgs()();
    }
}
