using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using NodaTime.Text;

namespace EasyDesk.CleanArchitecture.Web.Csv.Converters;

internal class NodaTimeConverter<T> : ITypeConverter
{
    private readonly IPattern<T> _pattern;

    public NodaTimeConverter(IPattern<T> pattern)
    {
        _pattern = pattern;
    }

    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }
        var result = _pattern.Parse(text);
        if (!result.Success)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, result.Exception.Message, result.Exception);
        }
        return result.Value;
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData) =>
        value is null ? null : _pattern.Format((T)value);
}
