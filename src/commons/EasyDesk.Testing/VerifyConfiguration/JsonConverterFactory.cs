using Argon;

namespace EasyDesk.Testing.VerifyConfiguration;

internal abstract class JsonConverterFactory : JsonConverter
{
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        return CreateConverter(type, serializer).ReadJson(reader, type, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        CreateConverter(value.GetType(), serializer).WriteJson(writer, value, serializer);
    }

    protected abstract JsonConverter CreateConverter(Type objectType, JsonSerializer serializer);
}
