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

    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) =>
        text is null ? null : _pattern.Parse(text).Value;

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData) =>
        value is null ? null : _pattern.Format((T)value);
}
