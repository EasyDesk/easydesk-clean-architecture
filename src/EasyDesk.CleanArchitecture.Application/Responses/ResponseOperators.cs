using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Responses;

public static partial class ResponseImports
{
    public static Response<A> IfSuccess<A>(this Response<A> response, Action<A> action)
    {
        response.Match(success: action);
        return response;
    }

    public static async Task<Response<A>> IfSuccessAsync<A>(this Response<A> response, AsyncAction<A> action)
    {
        await response.MatchAsync(success: action);
        return response;
    }

    public static Response<A> IfFailure<A>(this Response<A> response, Action<Error> action)
    {
        response.Match(failure: action);
        return response;
    }

    public static async Task<Response<A>> IfFailureAsync<A>(this Response<A> response, AsyncAction<Error> action)
    {
        await response.MatchAsync(failure: action);
        return response;
    }

    public static Response<A> Require<A>(this Response<A> response, Func<A, Response<Nothing>> requirement) =>
        response.FlatMap(a => requirement(a).Map(_ => a));

    public static Task<Response<A>> RequireAsync<A>(this Response<A> response, AsyncFunc<A, Response<Nothing>> requirement) =>
        response.FlatMapAsync(a => requirement(a).ThenMap(_ => a));

    public static Response<B> Map<A, B>(this Response<A> response, Func<A, B> mapper) =>
        response.FlatMap(x => Success(mapper(x)));

    public static Task<Response<B>> MapAsync<A, B>(this Response<A> response, AsyncFunc<A, B> mapper) =>
        response.FlatMapAsync(async x => Success(await mapper(x)));

    public static Response<A> MapError<A>(this Response<A> response, Func<Error, Error> mapper) => response.Match<Response<A>>(
        success: a => a,
        failure: e => mapper(e));

    public static Response<B> FlatMap<A, B>(this Response<A> response, Func<A, Response<B>> mapper) => response.Match(
        success: a => mapper(a),
        failure: e => e);

    public static Task<Response<B>> FlatMapAsync<A, B>(this Response<A> response, AsyncFunc<A, Response<B>> mapper) => response.Match(
        success: a => mapper(a),
        failure: e => Task.FromResult(Failure<B>(e)));
}
