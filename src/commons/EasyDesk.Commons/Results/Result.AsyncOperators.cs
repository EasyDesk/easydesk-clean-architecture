﻿using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static async Task<Result<A>> ThenIfSuccess<A>(this Task<Result<A>> result, Action<A> action) =>
        (await result).IfSuccess(action);

    public static async Task<Result<A>> ThenIfSuccessAsync<A>(this Task<Result<A>> result, AsyncAction<A> action) =>
        await (await result).IfSuccessAsync(action);

    public static async Task<Result<A>> ThenIfFailure<A>(this Task<Result<A>> result, Action<Error> action) =>
        (await result).IfFailure(action);

    public static async Task<Result<A>> ThenIfFailureAsync<A>(this Task<Result<A>> result, AsyncAction<Error> action) =>
        await (await result).IfFailureAsync(action);

    public static async Task<Result<A>> ThenFlatTap<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) =>
        (await result).FlatTap(mapper);

    public static async Task<Result<A>> ThenFlatTapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) =>
        await (await result).FlatTapAsync(mapper);

    public static async Task<Result<A>> ThenFilter<A>(this Task<Result<A>> result, Func<A, bool> predicate, Func<A, Error> otherwise) =>
        (await result).Filter(predicate, otherwise);

    public static async Task<Result<A>> ThenFilterAsync<A>(this Task<Result<A>> result, AsyncFunc<A, bool> predicate, Func<A, Error> otherwise) =>
        await (await result).FilterAsync(predicate, otherwise);

    public static async Task<Result<B>> ThenMap<A, B>(this Task<Result<A>> result, Func<A, B> mapper) =>
        (await result).Map(mapper);

    public static async Task<Result<B>> ThenMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, B> mapper) =>
        await (await result).MapAsync(mapper);

    public static async Task<Result<Nothing>> ThenIgnoreResult<A>(this Task<Result<A>> result) =>
        (await result).IgnoreResult();

    public static async Task<Result<A>> ThenMapError<A>(this Task<Result<A>> result, Func<Error, Error> mapper) =>
        (await result).MapError(mapper);

    public static async Task<Result<B>> ThenFlatMap<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) =>
        (await result).FlatMap(mapper);

    public static async Task<Result<B>> ThenFlatMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) =>
        await (await result).FlatMapAsync(mapper);

    public static async Task<A> ThenThrowIfFailure<A>(this Task<Result<A>> result) =>
        (await result).ThrowIfFailure();

    public static async Task<A> ThenThrowIfFailure<A>(this Task<Result<A>> result, Func<Error, Exception> exception) =>
        (await result).ThrowIfFailure(exception);

    public static async Task<B> ThenMatch<A, B>(this Task<Result<A>> result, Func<A, B> success, Func<Error, B> failure) =>
        (await result).Match(success, failure);

    public static async Task<B> ThenMatchAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, B> success, AsyncFunc<Error, B> failure) =>
        await (await result).MatchAsync(success, failure);

    public static async Task ThenMatch<A>(this Task<Result<A>> result, Action<A> success, Action<Error> failure) =>
        (await result).Match(success, failure);

    public static async Task ThenMatchAsync<A>(this Task<Result<A>> result, AsyncAction<A> success, AsyncAction<Error> failure) =>
        await (await result).MatchAsync(success, failure);

    public static async Task<Result<T>> ThenCatchError<T>(this Task<Result<T>> result, Func<Error, Result<T>> handler) =>
        (await result).CatchError(handler);

    public static async Task<Result<Nothing>> ThenCatchError(this Task<Result<Nothing>> result, Func<Error, bool> filter) =>
        (await result).CatchError(filter);

    public static async Task<Result<Nothing>> ThenCatchError<E>(this Task<Result<Nothing>> result, Func<E, bool> filter) where E : Error =>
        (await result).CatchError(filter);

    public static async Task<Result<T>> ThenCatchErrorAsync<T>(this Task<Result<T>> result, AsyncFunc<Error, Result<T>> handler) =>
        await (await result).CatchErrorAsync(handler);

    public static async Task<Result<Nothing>> ThenCatchErrorAsync(this Task<Result<Nothing>> result, AsyncFunc<Error, bool> filter) =>
        await (await result).CatchErrorAsync(filter);

    public static async Task<Result<Nothing>> ThenCatchErrorAsync<E>(this Task<Result<Nothing>> result, AsyncFunc<E, bool> filter) where E : Error =>
        await (await result).CatchErrorAsync(filter);
}
