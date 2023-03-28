namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Result<A> Require<A, B>(this Result<A> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.FlatTap(mapper);

    public static Task<Result<A>> RequireAsync<A, B>(this Result<A> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.FlatTapAsync(mapper);

    public static Task<Result<A>> ThenRequire<A, B>(this Task<Result<A>> result, Func<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.ThenFlatTap(mapper);

    public static Task<Result<A>> ThenRequireAsync<A, B>(this Task<Result<A>> result, AsyncFunc<A, Result<B>> mapper) where A : notnull where B : notnull =>
        result.ThenFlatTapAsync(mapper);
}
