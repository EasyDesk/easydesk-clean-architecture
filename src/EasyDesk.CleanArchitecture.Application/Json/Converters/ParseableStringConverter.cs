﻿using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Application.Json.Converters;

internal class ParseableStringConverter<T> : JsonConverter<T>
{
    private readonly Func<string, T> _parser;

    public ParseableStringConverter(Func<string, T> parser)
    {
        _parser = parser;
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return _parser(reader.Value as string);
    }
}