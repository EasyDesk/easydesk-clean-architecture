using System;

namespace EasyDesk.CleanArchitecture.Application.Json
{
    public interface IJsonSerializer
    {
        string Serialize(object value);

        object Deserialize(string json, Type type);
    }

    public static class JsonSerializerExtensions
    {
        public static T Deserialize<T>(this IJsonSerializer serializer, string json) =>
            (T)serializer.Deserialize(json, typeof(T));
    }
}
