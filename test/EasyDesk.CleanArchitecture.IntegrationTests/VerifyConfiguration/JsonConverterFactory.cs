using Argon;

namespace EasyDesk.CleanArchitecture.IntegrationTests.VerifyConfiguration;

internal abstract class JsonConverterFactory : JsonConverter
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return CreateConverter(objectType, serializer).ReadJson(reader, objectType, existingValue, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        CreateConverter(value.GetType(), serializer).WriteJson(writer, value, serializer);
    }

    protected abstract JsonConverter CreateConverter(Type objectType, JsonSerializer serializer);
}
