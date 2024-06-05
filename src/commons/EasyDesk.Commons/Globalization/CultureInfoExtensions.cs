using System.Globalization;

namespace EasyDesk.Commons.Globalization;

public static class CultureInfoExtensions
{
    public static void Use(this CultureInfo culture, Action action)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    public static T Use<T>(this CultureInfo culture, Func<T> func)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            return func();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    public static async Task UseAsync(this CultureInfo culture, Func<Task> action)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            await action();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    public static async Task<T> UseAsync<T>(this CultureInfo culture, Func<Task<T>> func)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = culture;
            return await func();
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }
}
