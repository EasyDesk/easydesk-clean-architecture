using CsvHelper;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public static class CsvExtensions
{
    public static T GetRequiredField<T>(this IReaderRow row, string name) where T : notnull
    {
        return row.GetField<Option<T>>(name).OrElseThrow(() => new MissingCsvValueException(name));
    }
}
