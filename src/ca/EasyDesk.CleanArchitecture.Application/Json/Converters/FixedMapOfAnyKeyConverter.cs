using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class FixedMapOfAnyKeyConverter : DictionaryOfAnyKeyConverter
{
    protected override JsonConverter CreateConverter(Type objectType)
    {
        var converterType = typeof(FixedMapOfAnyKeyConverterImpl<,>).MakeGenericType(objectType.GetGenericArguments());
        var innerConverter = base.CreateConverter(typeof(IImmutableDictionary<,>).MakeGenericType(objectType.GetGenericArguments()));
        return (JsonConverter)Activator.CreateInstance(converterType, innerConverter)!;
    }

    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
            && typeToConvert.IsSubtypeOrImplementationOf(typeof(IFixedMap<,>))
            && base.CanConvert(typeof(IImmutableDictionary<,>).MakeGenericType(typeToConvert.GetGenericArguments()));

    public class FixedMapOfAnyKeyConverterImpl<K, V> : JsonConverter<IFixedMap<K, V>>
        where K : notnull
    {
        private readonly JsonConverter<IImmutableDictionary<K, V>> _immutableDictionaryConverter;

        public FixedMapOfAnyKeyConverterImpl(JsonConverter<IImmutableDictionary<K, V>> immutableDictionaryConverter)
        {
            _immutableDictionaryConverter = immutableDictionaryConverter;
        }

        public override IFixedMap<K, V>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = _immutableDictionaryConverter.Read(ref reader, typeToConvert, options);
            return dictionary?.ToFixedMap();
        }

        public override void Write(Utf8JsonWriter writer, IFixedMap<K, V> value, JsonSerializerOptions options)
        {
            _immutableDictionaryConverter.Write(writer, value?.AsImmutableDictionary()!, options);
        }
    }
}
