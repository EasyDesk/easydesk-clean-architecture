using CsvHelper.TypeConversion;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Web.Csv;

public class CsvOptionConverterFactory : ITypeConverterFactory
{
    public bool CanCreate(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>);

    public bool Create(Type type, TypeConverterCache cache, out ITypeConverter typeConverter)
    {
        var wrappedType = type.GetGenericArguments()[0];
        var converterType = typeof(CsvOptionConverter<>).MakeGenericType(wrappedType);
        typeConverter = (Activator.CreateInstance(converterType, cache) as ITypeConverter)!;
        return true;
    }
}
