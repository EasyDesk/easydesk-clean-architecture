using System.Globalization;

namespace EasyDesk.Commons.Globalization;

public static class CultureInfoExtensions
{
    public static void Use(this CultureInfo culture, Action action) =>
        Use(culture, () =>
        {
            action();
            return Nothing.Value;
        });

    public static T Use<T>(this CultureInfo culture, Func<T> func)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var currentUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return func();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentUICulture;
        }
    }

    public static Task UseAsync(this CultureInfo culture, Func<Task> action) =>
        UseAsync(culture, async () =>
        {
            await action();
            return Nothing.Value;
        });

    public static async Task<T> UseAsync<T>(this CultureInfo culture, Func<Task<T>> func)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var currentUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return await func();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentUICulture;
        }
    }
}
