using EasyDesk.Commons.Strings;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Strings;

public class StringExtensionsTests
{
    [Theory]
    [MemberData(nameof(PrefixData))]
    public void ShouldRemovePrefixes(string text, string prefix, string expected)
    {
        text.RemovePrefix(prefix).ShouldBe(expected);
    }

    public static TheoryData<string, string, string> PrefixData() => new()
    {
        { "Hello World", "ello", "Hello World" },
        { "Hello World", "Hello", " World" },
        { "Hello World", "World", "Hello World" },
        { "Hello World", string.Empty, "Hello World" },
        { string.Empty, "World", string.Empty },
        { string.Empty, string.Empty, string.Empty },
        { "Hello World", "Hello World", string.Empty },
        { "Hello World", "Hello World!", "Hello World" },
    };

    [Theory]
    [MemberData(nameof(SuffixData))]
    public void ShouldRemoveSuffixes(string text, string suffix, string expected)
    {
        text.RemoveSuffix(suffix).ShouldBe(expected);
    }

    public static TheoryData<string, string, string> SuffixData() => new()
    {
        { "Hello World", "Worl", "Hello World" },
        { "Hello World", "World", "Hello " },
        { "Hello World", "Hello", "Hello World" },
        { "Hello World", string.Empty, "Hello World" },
        { string.Empty, "World", string.Empty },
        { string.Empty, string.Empty, string.Empty },
        { "Hello World", "Hello World", string.Empty },
        { "Hello World", "!Hello World", "Hello World" },
    };
}
