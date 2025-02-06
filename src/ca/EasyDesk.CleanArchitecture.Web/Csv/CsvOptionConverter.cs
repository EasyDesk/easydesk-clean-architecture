using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class CsvOptionConverter<T> : DefaultTypeConverter
{
    private readonly ITypeConverter _wrappedTypeConverter;

    public CsvOptionConverter(TypeConverterCache typeConverterCache)
    {
        _wrappedTypeConverter = typeConverterCache.GetConverter<T>();
    }

    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) =>
        GetOptionFromString(text, row, memberMapData);

    private Option<T> GetOptionFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text) || memberMapData.TypeConverterOptions.NullValues.Contains(text))
        {
            return None;
        }
        return _wrappedTypeConverter
            .ConvertFromString(text, row, memberMapData)
            .AsOption()
            .Map(x => (T)x);
    }
}
