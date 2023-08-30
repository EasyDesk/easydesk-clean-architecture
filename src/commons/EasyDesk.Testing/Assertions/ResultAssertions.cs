using EasyDesk.Commons.Results;
using Shouldly;

namespace EasyDesk.Testing.Assertions;

public static class ResultAssertions
{
    public static void ShouldBeSuccess<T>(this Result<T> result) =>
        result.IsSuccess.ShouldBeTrue();

    public static void ShouldBeFailure<T>(this Result<T> result) =>
        result.IsFailure.ShouldBeTrue();
}
