using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Results;

public static partial class ResultImports
{
    public static Result<A> IfSuccess<A>(this Result<A> result, Action<A> action)
    {
        result.Match(success: action);
        return result;
    }

    public static async Task<Result<A>> IfSuccessAsync<A>(this Result<A> result, AsyncAction<A> action)
    {
        await result.MatchAsync(success: action);
        return result;
    }

    public static Result<A> IfFailure<A>(this Result<A> result, Action<DomainError> action)
    {
        result.Match(failure: action);
        return result;
    }

    public static async Task<Result<A>> IfFailureAsync<A>(this Result<A> result, AsyncAction<DomainError> action)
    {
        await result.MatchAsync(failure: action);
        return result;
    }

    public static Result<A> Require<A>(this Result<A> result, Func<A, Result<Nothing>> requirement) =>
        result.FlatMap(a => requirement(a).Map(_ => a));

    public static Task<Result<A>> RequireAsync<A>(this Result<A> result, AsyncFunc<A, Result<Nothing>> requirement) =>
        result.FlatMapAsync(a => requirement(a).ThenMap(_ => a));

    public static Result<B> Map<A, B>(this Result<A> result, Func<A, B> mapper) =>
        result.FlatMap(x => Success(mapper(x)));

    public static Task<Result<B>> MapAsync<A, B>(this Result<A> result, AsyncFunc<A, B> mapper) =>
        result.FlatMapAsync(async x => Success(await mapper(x)));

    public static Result<A> MapError<A>(this Result<A> result, Func<DomainError, DomainError> mapper) => result.Match<Result<A>>(
        success: a => a,
        failure: e => mapper(e));

    public static Result<B> FlatMap<A, B>(this Result<A> result, Func<A, Result<B>> mapper) => result.Match(
        success: a => mapper(a),
        failure: e => e);

    public static Task<Result<B>> FlatMapAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) => result.Match(
        success: a => mapper(a),
        failure: e => Task.FromResult(Failure<B>(e)));
}
