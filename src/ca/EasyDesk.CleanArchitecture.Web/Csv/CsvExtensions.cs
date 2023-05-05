using CsvHelper;
using System.Diagnostics;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public static class CsvExtensions
{
    public static T GetRequiredField<T>(this IReaderRow row, string name) where T : notnull
    {
        return row.GetField<T>(name) ?? throw new UnreachableException();
    }

    public static Option<T> GetOptionalField<T>(this IReaderRow row, string name) where T : notnull
    {
        return row.GetField(name)
            .AsOption()
            .Filter(x => !string.IsNullOrEmpty(x))
            .Map(_ => row.GetRequiredField<T>(name));
    }
}
