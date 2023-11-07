using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons;

public static partial class StaticImports
{
    public static T It<T>(T t) => t;

    public static R Let<T, R>(this T self, Func<T, R> function) => function(self);

    public static async Task<R> Let<T, R>(this T self, AsyncFunc<T, R> function) => await function(self);

    public static T Also<T>(this T self, Action<T> action)
    {
        action(self);
        return self;
    }

    public static async Task<T> Also<T>(this T self, AsyncAction<T> action)
    {
        await action(self);
        return self;
    }

    public static Nothing ReturningNothing(Action action)
    {
        action();
        return Nothing.Value;
    }

    public static async Task<Nothing> ReturningNothing(AsyncAction action)
    {
        await action();
        return Nothing.Value;
    }
}
