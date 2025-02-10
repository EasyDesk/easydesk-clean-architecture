using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static Option<T> IfPresent<T>(this Option<T> option, Action<T> action)
    {
        option.Match(
            some: action,
            none: () => { });
        return option;
    }

    public static async Task<Option<T>> IfPresentAsync<T>(this Option<T> option, AsyncAction<T> action)
    {
        await option.MatchAsync(
            some: action,
            none: () => Task.CompletedTask);
        return option;
    }

    public static bool IsPresent<T>(this Option<T> option, out T value)
    {
        value = default!;
        if (option.IsPresent)
        {
            value = option.Value;
            return true;
        }
        return false;
    }

    public static Option<T> IfAbsent<T>(this Option<T> option, Action action)
    {
        option.Match(
            some: _ => { },
            none: action);
        return option;
    }

    public static async Task<Option<T>> IfAbsentAsync<T>(this Option<T> option, AsyncAction action)
    {
        await option.MatchAsync(
            some: _ => Task.CompletedTask,
            none: action);
        return option;
    }

    public static bool IsAbsent<T>(this Option<T> option, out T value) =>
        !option.IsPresent(out value);

    public static Option<R> Map<T, R>(this Option<T> option, Func<T, R> mapper) => option.Match(
        some: t => Some(mapper(t)),
        none: () => None);

    public static Option<string> MapToString<T>(this Option<T> option) => option.FlatMap(o => (o?.ToString()).AsOption());

    public static Task<Option<R>> MapAsync<T, R>(this Option<T> option, AsyncFunc<T, R> mapper) => option.MatchAsync(
        some: async t => Some(await mapper(t)),
        none: () => Task.FromResult<Option<R>>(None));

    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate) => option.Match(
        some: t => predicate(t) ? option : None,
        none: () => None);

    public static Task<Option<T>> FilterAsync<T>(this Option<T> option, AsyncFunc<T, bool> predicate) => option.MatchAsync(
        some: async t => (await predicate(t)) ? option : None,
        none: () => Task.FromResult<Option<T>>(None));

    public static Option<R> FlatMap<T, R>(this Option<T> option, Func<T, Option<R>> mapper) => option.Match(
        some: mapper,
        none: () => None);

    public static Task<Option<R>> FlatMapAsync<T, R>(this Option<T> option, AsyncFunc<T, Option<R>> mapper) => option.MatchAsync(
        some: mapper,
        none: () => Task.FromResult<Option<R>>(None));

    public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
        option.FlatMap(o => o);

    public static Option<T> Or<T>(this Option<T> option, Option<T> other)
    {
        return option.Match(
            some: _ => option,
            none: () => other);
    }

    public static bool Contains<T>(this Option<T> option, Func<T, bool> proposition) =>
        option.Match(some: proposition, none: () => false);

    public static Task<bool> ContainsAsync<T>(this Option<T> option, AsyncFunc<T, bool> proposition) =>
        option.MatchAsync(some: proposition, none: () => Task.FromResult(false));
}
