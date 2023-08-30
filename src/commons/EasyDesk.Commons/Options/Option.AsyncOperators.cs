using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static async Task<Option<T>> ThenIfPresent<T>(this Task<Option<T>> option, Action<T> action) =>
        (await option).IfPresent(action);

    public static async Task<Option<T>> ThenIfPresentAsync<T>(this Task<Option<T>> option, AsyncAction<T> action) =>
        await (await option).IfPresentAsync(action);

    public static async Task<Option<T>> ThenIfAbsent<T>(this Task<Option<T>> option, Action action) =>
        (await option).IfAbsent(action);

    public static async Task<Option<T>> ThenIfAbsentAsync<T>(this Task<Option<T>> option, AsyncAction action) =>
        await (await option).IfAbsentAsync(action);

    public static async Task<Option<R>> ThenMap<T, R>(this Task<Option<T>> option, Func<T, R> mapper) =>
        (await option).Map(mapper);

    public static async Task<Option<R>> ThenMapAsync<T, R>(this Task<Option<T>> option, AsyncFunc<T, R> mapper) =>
        await (await option).MapAsync(mapper);

    public static async Task<Option<T>> ThenFilter<T>(this Task<Option<T>> option, Func<T, bool> predicate) =>
        (await option).Filter(predicate);

    public static async Task<Option<T>> ThenFilterAsync<T>(this Task<Option<T>> option, AsyncFunc<T, bool> predicate) =>
        await (await option).FilterAsync(predicate);

    public static async Task<Option<R>> ThenFlatMap<T, R>(this Task<Option<T>> option, Func<T, Option<R>> mapper) =>
        (await option).FlatMap(mapper);

    public static async Task<Option<R>> ThenFlatMapAsync<T, R>(this Task<Option<T>> option, AsyncFunc<T, Option<R>> mapper) =>
        await (await option).FlatMapAsync(mapper);
}
