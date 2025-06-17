using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class FixedSetConverter : CachedJsonConverterFactory
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedSetConverterImpl<>).MakeGenericType(objectType.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.IsSubtypeOrImplementationOf(typeof(IFixedSet<>));

    public class FixedSetConverterImpl<T> : JsonConverter<IFixedSet<T>>
    {
        public override IFixedSet<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var set = JsonSerializer.Deserialize<ImmutableHashSet<T>>(ref reader, options);
            return set is null ? null : FixedHashSet.Create(set);
        }

        public override void Write(Utf8JsonWriter writer, IFixedSet<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value?.AsImmutableSet(), options);
        }
    }
}
