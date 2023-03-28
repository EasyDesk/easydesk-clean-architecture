using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using Shouldly;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.ErrorManagement;

public class ErrorsTests
{
    [Theory]
    [MemberData(nameof(GenericErrorData))]
    public void Generic_ShouldReplaceWordsInCurlyBracesWithTheRespectiveValue(
        string messageFormat, string expectedMessage, IImmutableDictionary<string, object> expectedParams, params object[] args)
    {
        var error = Errors.Generic(messageFormat, args);

        error.ShouldBe(new GenericError(expectedMessage, expectedParams));
    }

    public static IEnumerable<object[]> GenericErrorData()
    {
        var arg1 = 10;
        var arg2 = "Hello";

        var message = "Hello World";
        yield return new object[]
        {
                message,
                message,
                Map<string, object>()
        };
        yield return new object[]
        {
                "First value {value1}, Second value {value2}",
                $"First value {arg1}, Second value {arg2}",
                Map<string, object>(("value1", arg1), ("value2", arg2)),
                arg1, arg2
        };
        yield return new object[]
        {
                "{value1}{value2}{value1}",
                $"{arg1}{arg2}{arg1}",
                Map<string, object>(("value1", arg1), ("value2", arg2)),
                arg1, arg2
        };
        yield return new object[]
        {
                "{value1} {value1} {value2}",
                $"{arg1} {arg1} {arg2}",
                Map<string, object>(("value1", arg1), ("value2", arg2)),
                arg1, arg2
        };
        yield return new object[]
        {
                "{value1:D5} {value1:C1}",
                $"{arg1:D5} {arg1:C1}",
                Map<string, object>(("value1", arg1)),
                arg1
        };
        yield return new object[]
        {
                @"\{value}",
                "{value}",
                Map<string, object>()
        };
    }

    [Theory]
    [MemberData(nameof(NotEnoughArgumentsData))]
    public void Generic_ShouldFail_IfNotEnoughArgumentsArePassed(
        string messageFormat, params object[] args)
    {
        Should.Throw<Exception>(() => Errors.Generic(messageFormat, args));
    }

    public static IEnumerable<object[]> NotEnoughArgumentsData()
    {
        yield return new object[] { "{value}" };
        yield return new object[] { "{value1} {value2}", 10 };
        yield return new object[] { "{value1} {value2} {value1}", 10 };
    }
}
