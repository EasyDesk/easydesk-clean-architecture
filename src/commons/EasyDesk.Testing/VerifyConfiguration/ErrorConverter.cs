using Argon;

namespace EasyDesk.Testing.VerifyConfiguration;

public class ErrorConverter : WriteOnlyJsonConverter<Error>
{
    public override void Write(VerifyJsonWriter writer, Error value)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        writer.WriteStartObject();
        writer.WritePropertyName("Error");
        writer.WriteValue(value.GetType().Name);
        writer.WritePropertyName("Meta");
        if (value is MultiError multiError)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Primary");
            writer.Serialize(multiError.PrimaryError);
            writer.WritePropertyName("Secondary");
            writer.WriteStartArray();
            foreach (var subError in multiError.SecondaryErrors.OrderBy(e => (e.GetType().Name, e.GetHashCode())))
            {
                writer.Serialize(subError);
            }
            writer.WriteEndArray();
        }
        else
        {
            var settings = CloneSettingsFromSerializer(writer.Serializer);
            var nestedSerializer = JsonSerializer.Create(settings);
            nestedSerializer.Serialize(writer, value);
        }
        writer.WriteEndObject();
    }

    private JsonSerializerSettings CloneSettingsFromSerializer(JsonSerializer serializer)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            CheckAdditionalContent = serializer.CheckAdditionalContent,
            ConstructorHandling = serializer.ConstructorHandling,
            ContractResolver = serializer.ContractResolver,
            Converters = new List<JsonConverter>(serializer.Converters),
            DefaultValueHandling = serializer.DefaultValueHandling,
            EqualityComparer = serializer.EqualityComparer,
            Error = serializer.Error,
            EscapeHandling = serializer.EscapeHandling,
            FloatFormatHandling = serializer.FloatFormatHandling,
            FloatParseHandling = serializer.FloatParseHandling,
            Formatting = serializer.Formatting,
            MaxDepth = serializer.MaxDepth,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            MissingMemberHandling = serializer.MissingMemberHandling,
            NullValueHandling = serializer.NullValueHandling,
            ObjectCreationHandling = serializer.ObjectCreationHandling,
            PreserveReferencesHandling = serializer.PreserveReferencesHandling,
            ReferenceLoopHandling = serializer.ReferenceLoopHandling,
            ReferenceResolverProvider = () => serializer.ReferenceResolver,
            SerializationBinder = serializer.SerializationBinder,
            TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling,
            TypeNameHandling = serializer.TypeNameHandling,
        };
        serializerSettings.Converters.Remove(this);
        return serializerSettings;
    }
}
