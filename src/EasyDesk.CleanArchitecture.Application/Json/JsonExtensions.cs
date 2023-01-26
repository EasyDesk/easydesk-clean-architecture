using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace EasyDesk.CleanArchitecture.Application.Json;

public static class JsonExtensions
{
    public static byte[] SerializeToBsonBytes(this JsonSerializer serializer, object? value)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BsonDataWriter(memoryStream);

        serializer.Serialize(writer, value);

        return memoryStream.ToArray();
    }

    public static object? DeserializeFromBsonBytes(this JsonSerializer serializer, byte[] bytes, Type type)
    {
        using var memoryStream = new MemoryStream(bytes);
        using var reader = new BsonDataReader(memoryStream);

        return serializer.Deserialize(reader, type);
    }

    public static T? DeserializeFromBsonBytes<T>(this JsonSerializer serializer, byte[] bytes) =>
        (T?)serializer.DeserializeFromBsonBytes(bytes, typeof(T));
}
