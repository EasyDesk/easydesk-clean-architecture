namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Result<A> IfSuccess<A>(this Result<A> result, Action<A> action) where A : notnull
    {
        result.Match(success: action);
        return result;
    }

    public static async Task<Result<A>> IfSuccessAsync<A>(this Result<A> result, AsyncAction<A> action) where A : notnull
    {
        await result.MatchAsync(success: action);
        return result;
    }

    public static Result<A> IfFailure<A>(this Result<A> result, Action<Error> action) where A : notnull
    {
        result.Match(failure: action);
        return result;
    }

    public static async Task<Result<A>> IfFailureAsync<A>(this Result<A> result, AsyncAction<Error> action) where A : notnull
    {
        await result.MatchAsync(failure: action);
        return result;
    }

    public static Result<A> FlatTap<A, B>(this Result<A> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.FlatMap(a => mapper(a).Map(_ => a));

    public static Task<Result<A>> FlatTapAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.FlatMapAsync(a => mapper(a).ThenMap(_ => a));

    public static Result<A> Filter<A>(this Result<A> result, Func<A, bool> predicate, Func<A, Error> otherwise) where A : notnull =>
        result.FlatTap(a => Ensure(predicate(a), () => otherwise(a)));

    public static Task<Result<A>> FilterAsync<A>(this Result<A> result, AsyncFunc<A, bool> predicate, Func<A, Error> otherwise) where A : notnull =>
        result.FlatTapAsync(async a => Ensure(await predicate(a), () => otherwise(a)));

    public static Result<B> Map<A, B>(this Result<A> result, Func<A, B> mapper) where A : notnull where B : notnull =>
        result.FlatMap(x => Success(mapper(x)));

    public static Result<Nothing> IgnoreResult<A>(this Result<A> result) where A : notnull =>
        result.Map(_ => Nothing.Value);

    public static Task<Result<B>> MapAsync<A, B>(this Result<A> result, AsyncFunc<A, B> mapper) where A : notnull where B : notnull =>
        result.FlatMapAsync(async x => Success(await mapper(x)));

    public static Result<A> MapError<A>(this Result<A> result, Func<Error, Error> mapper) where A : notnull => result.Match(
        success: a => a,
        failure: e => Failure<A>(mapper(e)));

    public static Result<B> FlatMap<A, B>(this Result<A> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull => result.Match(
        success: a => mapper(a),
        failure: Failure<B>);

    public static Task<Result<B>> FlatMapAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull => result.Match(
        success: a => mapper(a),
        failure: e => Task.FromResult(Failure<B>(e)));

    public static A ThrowIfFailure<A>(this Result<A> result, Func<Error, Exception> exception) where A : notnull => result.Match(
        success: a => a,
        failure: e => throw exception(e));

    public static A ThrowIfFailure<A>(this Result<A> result) where A : notnull => result.ThrowIfFailure(e => new ResultFailedException(e));
}
