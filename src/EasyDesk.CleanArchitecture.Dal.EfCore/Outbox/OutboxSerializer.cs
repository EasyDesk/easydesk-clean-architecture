using EasyDesk.CleanArchitecture.Infrastructure.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class OutboxSerializer
{
    private readonly JsonSerializer _serializer;

    public OutboxSerializer()
    {
        var settings = JsonDefaults.DefaultSerializerSettings();
        _serializer = JsonSerializer.Create(settings);
    }

    public byte[] Serialize(object data)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BsonDataWriter(memoryStream);
        _serializer.Serialize(writer, data);

        return memoryStream.ToArray();
    }

    public object Deserialize(byte[] binaryData, Type type)
    {
        using var memoryStream = new MemoryStream(binaryData);
        using var reader = new BsonDataReader(memoryStream);

        return _serializer.Deserialize(reader, type);
    }

    public T Deserialize<T>(byte[] binaryData) => (T)Deserialize(binaryData, typeof(T));
}
