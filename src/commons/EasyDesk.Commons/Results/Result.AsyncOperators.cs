namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static async Task<Result<A>> ThenIfSuccess<A>(this Task<Result<A>> result, Action<A> action) where A : notnull =>
        (await result).IfSuccess(action);

    public static async Task<Result<A>> ThenIfSuccessAsync<A>(this Task<Result<A>> result, AsyncAction<A> action) where A : notnull =>
        await (await result).IfSuccessAsync(action);

    public static async Task<Result<A>> ThenIfFailure<A>(this Task<Result<A>> result, Action<Error> action) where A : notnull =>
        (await result).IfFailure(action);

    public static async Task<Result<A>> ThenIfFailureAsync<A>(this Task<Result<A>> result, AsyncAction<Error> action) where A : notnull =>
        await (await result).IfFailureAsync(action);

    public static async Task<Result<A>> ThenFlatTap<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull =>
        (await result).FlatTap(mapper);

    public static async Task<Result<A>> ThenFlatTapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull =>
        await (await result).FlatTapAsync(mapper);

    public static async Task<Result<A>> ThenFilter<A>(this Task<Result<A>> result, Func<A, bool> predicate, Func<A, Error> otherwise) where A : notnull =>
        (await result).Filter(predicate, otherwise);

    public static async Task<Result<A>> ThenFilterAsync<A>(this Task<Result<A>> result, AsyncFunc<A, bool> predicate, Func<A, Error> otherwise) where A : notnull =>
        await (await result).FilterAsync(predicate, otherwise);

    public static async Task<Result<B>> ThenMap<A, B>(this Task<Result<A>> result, Func<A, B> mapper) where A : notnull where B : notnull =>
        (await result).Map(mapper);

    public static async Task<Result<B>> ThenMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, B> mapper) where A : notnull where B : notnull =>
        await (await result).MapAsync(mapper);

    public static async Task<Result<Nothing>> ThenIgnoreResult<A>(this Task<Result<A>> result) where A : notnull =>
        (await result).IgnoreResult();

    public static async Task<Result<A>> ThenMapError<A>(this Task<Result<A>> result, Func<Error, Error> mapper) where A : notnull =>
        (await result).MapError(mapper);

    public static async Task<Result<B>> ThenFlatMap<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull =>
        (await result).FlatMap(mapper);

    public static async Task<Result<B>> ThenFlatMapAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull =>
        await (await result).FlatMapAsync(mapper);

    public static async Task<A> ThenThrowIfFailure<A>(this Task<Result<A>> result) where A : notnull =>
        (await result).ThrowIfFailure();

    public static async Task<A> ThenThrowIfFailure<A>(this Task<Result<A>> result, Func<Error, Exception> exception) where A : notnull =>
        (await result).ThrowIfFailure(exception);
}
