using EasyDesk.CleanArchitecture.Application.Json;
using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public NewtonsoftJsonSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public string Serialize(object value) => JsonConvert.SerializeObject(value, _serializerSettings);

        public object Deserialize(string json, Type type) => JsonConvert.DeserializeObject(json, type, _serializerSettings);
    }
}
