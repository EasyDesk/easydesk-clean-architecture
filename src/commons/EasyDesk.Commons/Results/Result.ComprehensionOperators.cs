namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Result<R> Select<T, R>(this Result<T> result, Func<T, R> mapper) where T : notnull where R : notnull =>
        result.Map(mapper);

    public static Result<R> SelectMany<T, R>(this Result<T> result, Func<T, Result<R>> mapper) where T : notnull where R : notnull =>
        result.FlatMap(mapper);

    public static Result<R> SelectMany<T, X, R>(this Result<T> result, Func<T, Result<X>> mapper, Func<T, X, R> project) where T : notnull where X : notnull where R : notnull =>
        result.FlatMap(x => mapper(x).Map(y => project(x, y)));
}
