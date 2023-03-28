using EasyDesk.Commons;
using System.Linq;
using VerifyTests;

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
            var thisIndex = writer.Serializer.Converters.IndexOf(this);
            writer.Serializer.Converters.RemoveAt(thisIndex);
            writer.Serialize(value);
            writer.Serializer.Converters.Insert(thisIndex, this);
        }
        writer.WriteEndObject();
    }
}
