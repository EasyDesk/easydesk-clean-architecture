using System.Reflection;

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
            writer.WriteStartObject();
            foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                writer.WritePropertyName(property.Name);
                var propertyValue = property.GetValue(value);
                if (propertyValue is null)
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.Serialize(propertyValue);
                }
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }
}
