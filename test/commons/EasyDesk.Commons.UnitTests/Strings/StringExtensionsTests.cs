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

    public static IEnumerable<object[]> PrefixData()
    {
        yield return new[] { "Hello World", "ello", "Hello World" };
        yield return new[] { "Hello World", "Hello", " World" };
        yield return new[] { "Hello World", "World", "Hello World" };
        yield return new[] { "Hello World", string.Empty, "Hello World" };
        yield return new[] { string.Empty, "World", string.Empty };
        yield return new[] { string.Empty, string.Empty, string.Empty };
        yield return new[] { "Hello World", "Hello World", string.Empty };
        yield return new[] { "Hello World", "Hello World!", "Hello World" };
    }

    [Theory]
    [MemberData(nameof(SuffixData))]
    public void ShouldRemoveSuffixes(string text, string suffix, string expected)
    {
        text.RemoveSuffix(suffix).ShouldBe(expected);
    }

    public static IEnumerable<object[]> SuffixData()
    {
        yield return new[] { "Hello World", "Worl", "Hello World" };
        yield return new[] { "Hello World", "World", "Hello " };
        yield return new[] { "Hello World", "Hello", "Hello World" };
        yield return new[] { "Hello World", string.Empty, "Hello World" };
        yield return new[] { string.Empty, "World", string.Empty };
        yield return new[] { string.Empty, string.Empty, string.Empty };
        yield return new[] { "Hello World", "Hello World", string.Empty };
        yield return new[] { "Hello World", "!Hello World", "Hello World" };
    }
}
