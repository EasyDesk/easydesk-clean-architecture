using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static T OrElseGet<T>(this Option<T> option, Func<T> supplier) =>
        option.Match(some: t => t, none: supplier);

    public static Task<T> OrElseGetAsync<T>(this Option<T> option, AsyncFunc<T> supplier) =>
        option.MatchAsync(some: t => Task.FromResult(t), none: supplier);

    public static T OrElse<T>(this Option<T> option, T defaultValue) =>
        option.OrElseGet(() => defaultValue);

    public static T? OrElseDefault<T>(this Option<T> option) =>
        option.Match<T?>(some: t => t, none: () => default);

    public static T? OrElseNull<T>(this Option<T> option) where T : class =>
        option.Match<T?>(some: t => t, none: () => null);

    public static T? OrElseNothing<T>(this Option<T> option) where T : struct =>
        option.Match<T?>(some: t => t, none: () => null);

    public static T OrElseThrow<T>(this Option<T> option, Func<Exception> exceptionSupplier) =>
        option.OrElseGet(() => throw exceptionSupplier());
}
