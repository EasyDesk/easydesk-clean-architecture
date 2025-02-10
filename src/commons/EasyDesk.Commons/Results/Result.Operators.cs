using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Result<T> IfSuccess<T>(this Result<T> result, Action<T> action)
    {
        result.Match(success: action);
        return result;
    }

    public static async Task<Result<T>> IfSuccessAsync<T>(this Result<T> result, AsyncAction<T> action)
    {
        await result.MatchAsync(success: action);
        return result;
    }

    public static bool IsSuccess<T>(this Result<T> result, out T value, out Error error)
    {
        value = default!;
        error = default!;
        if (result.IsSuccess)
        {
            value = result.ReadValue();
            return true;
        }
        error = result.ReadError();
        return false;
    }

    public static bool IsSuccess<T>(this Result<T> result, out T value) => result.IsSuccess(out value, out _);

    public static Result<T> IfFailure<T>(this Result<T> result, Action<Error> action)
    {
        result.Match(failure: action);
        return result;
    }

    public static async Task<Result<T>> IfFailureAsync<T>(this Result<T> result, AsyncAction<Error> action)
    {
        await result.MatchAsync(failure: action);
        return result;
    }

    public static bool IsFailure<T>(this Result<T> result, out T value, out Error error) =>
        !result.IsSuccess(out value, out error);

    public static bool IsFailure<T>(this Result<T> result, out T value) => result.IsFailure(out value, out _);

    public static Result<A> FlatTap<A, B>(this Result<A> result, Func<A, Result<B>> mapper) =>
        result.FlatMap(a => mapper(a).Map(_ => a));

    public static Task<Result<A>> FlatTapAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) =>
        result.FlatMapAsync(a => mapper(a).ThenMap(_ => a));

    public static Result<A> Filter<A>(this Result<A> result, Func<A, bool> predicate, Func<A, Error> otherwise) =>
        result.FlatTap(a => Ensure(predicate(a), () => otherwise(a)));

    public static Task<Result<A>> FilterAsync<A>(this Result<A> result, AsyncFunc<A, bool> predicate, Func<A, Error> otherwise) =>
        result.FlatTapAsync(async a => Ensure(await predicate(a), () => otherwise(a)));

    public static Result<B> Map<A, B>(this Result<A> result, Func<A, B> mapper) =>
        result.FlatMap(x => Success(mapper(x)));

    public static Result<Nothing> IgnoreResult<A>(this Result<A> result) =>
        result.Map(_ => Nothing.Value);

    public static Task<Result<B>> MapAsync<A, B>(this Result<A> result, AsyncFunc<A, B> mapper) =>
        result.FlatMapAsync(async x => Success(await mapper(x)));

    public static Result<A> MapError<A>(this Result<A> result, Func<Error, Error> mapper) => result.Match(
        success: a => a,
        failure: e => Failure<A>(mapper(e)));

    public static Result<B> FlatMap<A, B>(this Result<A> result, Func<A, Result<B>> mapper) => result.Match(
        success: a => mapper(a),
        failure: Failure<B>);

    public static Task<Result<B>> FlatMapAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) => result.Match(
        success: a => mapper(a),
        failure: e => Task.FromResult(Failure<B>(e)));

    public static A ThrowIfFailure<A>(this Result<A> result, Func<Error, Exception> exception) => result.Match(
        success: a => a,
        failure: e => throw exception(e));

    public static A ThrowIfFailure<A>(this Result<A> result) => result.ThrowIfFailure(e => new ResultFailedException(e));

    public static bool Contains<T>(this Result<T> result, Func<T, bool> proposition) =>
        result.Match(success: proposition, failure: _ => false);

    public static Task<bool> ContainsAsync<T>(this Result<T> result, AsyncFunc<T, bool> proposition) =>
        result.MatchAsync(success: proposition, failure: _ => Task.FromResult(false));

    public static Result<IEnumerable<T>> CatchFirstFailure<T>(this IEnumerable<Result<T>> enumerable) => enumerable
        .FirstOption(r => r.IsFailure)
        .Match(
            some: r => r.ReadError(),
            none: () => Success(enumerable.Select(r => r.ReadValue())));

    public static Result<IEnumerable<T>> CatchAllFailures<T>(this IEnumerable<Result<T>> enumerable) => enumerable
        .SelectMany(r => r.Error)
        .AsSome()
        .Map(l => l.ToList())
        .Filter(l => l.Any())
        .Map(l => new MultiError(l[0], l.Skip(1).ToFixedList()))
        .Map(Failure<IEnumerable<T>>)
        .OrElseGet(() => Success(enumerable.Select(r => r.ReadValue())));

    public static Result<Option<T>> SingleOptionOrError<T>(this IEnumerable<T> enumerable, Func<Error> otherwise)
    {
        using (var enumerator = enumerable.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                return NoneT<T>();
            }
            var itemToReturn = enumerator.Current;
            if (enumerator.MoveNext())
            {
                return otherwise();
            }
            return Some(itemToReturn);
        }
    }
}
