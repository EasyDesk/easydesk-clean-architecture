using CsvHelper;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public static class CsvExtensions
{
    public static T GetRequiredField<T>(this IReaderRow row, string name) where T : notnull
    {
        return row.GetOptionalField<T>(name).OrElseThrow(() => new MissingCsvValueException(name));
    }

    public static Option<T> GetOptionalField<T>(this IReaderRow row, string name) where T : notnull
    {
        return row.Exists(name) ? row.GetField<T?>(name).AsOption() : None;
    }

    private static bool Exists(this IReaderRow row, string name)
    {
        return row.GetField(name)
            .AsOption()
            .Filter(x => !string.IsNullOrEmpty(x))
            .IsPresent;
    }
}
