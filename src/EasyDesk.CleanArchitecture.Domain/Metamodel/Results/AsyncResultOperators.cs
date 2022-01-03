using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Results;

public static partial class ResultImports
{
    public static async Task<Result<A>> ThenIfSuccess<A>(this Task<Result<A>> result, Action<A> action) =>
        (await result).IfSuccess(action);

    public static async Task<Result<A>> ThenIfSuccessAsync<A>(this Task<Result<A>> result, AsyncAction<A> action) =>
        await (await result).IfSuccessAsync(action);

    public static async Task<Result<A>> ThenIfFailure<A>(this Task<Result<A>> result, Action<DomainError> action) =>
        (await result).IfFailure(action);

    public static async Task<Result<A>> ThenIfFailureAsync<A>(this Task<Result<A>> result, AsyncAction<DomainError> action) =>
        await (await result).IfFailureAsync(action);

    public static async Task<Result<A>> ThenRequire<A>(this Task<Result<A>> result, Func<A, Result<Nothing>> requirement) =>
        (await result).Require(requirement);

    public static async Task<Result<A>> ThenRequireAsync<A>(this Task<Result<A>> result, AsyncFunc<A, Result<Nothing>> requirement) =>
        await (await result).RequireAsync(requirement);

    public static async Task<Result<B>> ThenMap<A, B>(this Task<Result<A>> result, Func<A, B> mapper) =>
        (await result).Map(mapper);

    public static async Task<Result<B>> ThenMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, B> mapper) =>
        await (await result).MapAsync(mapper);

    public static async Task<Result<A>> ThenMapDomainError<A>(this Task<Result<A>> result, Func<DomainError, DomainError> mapper) =>
        (await result).MapError(mapper);

    public static async Task<Result<B>> ThenFlatMap<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) =>
        (await result).FlatMap(mapper);

    public static async Task<Result<B>> ThenFlatMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) =>
        await (await result).FlatMapAsync(mapper);
}
