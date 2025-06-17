using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class FixedListConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedListConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.IsSubtypeOrImplementationOf(typeof(IFixedList<>));

    public class FixedListConverterImpl<T> : JsonConverter<IFixedList<T>>
    {
        public override IFixedList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = JsonSerializer.Deserialize<ImmutableList<T>>(ref reader, options);
            return list is null ? null : FixedList.Create(list);
        }

        public override void Write(Utf8JsonWriter writer, IFixedList<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value?.AsImmutableList(), options);
        }
    }
}
