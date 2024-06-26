﻿using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Collections.Immutable;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.ErrorManagement;

public class ErrorsTests
{
    [Theory]
    [MemberData(nameof(GenericErrorData))]
    public void Generic_ShouldReplaceWordsInCurlyBracesWithTheRespectiveValue(
        string messageFormat, string expectedMessage, IFixedMap<string, object> expectedParams, params object[] args)
    {
        var code = "ErrorCode";
        var error = Errors.Generic(code, messageFormat, args);

        error.ShouldBe(new GenericError(code, expectedMessage, expectedParams));
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
            Map<string, object>(),
        };
        yield return new object[]
        {
            "First value {value1}, Second value {value2}",
            $"First value {arg1}, Second value {arg2}",
            Map<string, object>(("value1", arg1), ("value2", arg2)),
            arg1, arg2,
        };
        yield return new object[]
        {
            "{value1}{value2}{value1}",
            $"{arg1}{arg2}{arg1}",
            Map<string, object>(("value1", arg1), ("value2", arg2)),
            arg1, arg2,
        };
        yield return new object[]
        {
            "{value1} {value1} {value2}",
            $"{arg1} {arg1} {arg2}",
            Map<string, object>(("value1", arg1), ("value2", arg2)),
            arg1, arg2,
        };
        yield return new object[]
        {
            "{value1:D5} {value1:C1}",
            $"{arg1:D5} {arg1:C1}",
            Map<string, object>(("value1", arg1)),
            arg1,
        };
        yield return new object[]
        {
            @"\{value}",
            "{value}",
            Map<string, object>(),
        };
    }

    [Theory]
    [MemberData(nameof(NotEnoughArgumentsData))]
    public void Generic_ShouldFail_IfNotEnoughArgumentsArePassed(
        string messageFormat, params object[] args)
    {
        Should.Throw<Exception>(() => Errors.Generic("Code", messageFormat, args));
    }

    public static IEnumerable<object[]> NotEnoughArgumentsData()
    {
        yield return new object[] { "{value}" };
        yield return new object[] { "{value1} {value2}", 10 };
        yield return new object[] { "{value1} {value2} {value1}", 10 };
    }
}
