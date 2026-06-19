using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EasyDesk.SampleApp.Web;

public class TypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public const string PolymorphicTypeDiscriminator = "Discriminator";

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var defaultInfo = base.GetTypeInfo(type, options);

        if (type == typeof(BasePolymorphicDto))
        {
            defaultInfo.PolymorphismOptions = new()
            {
                IgnoreUnrecognizedTypeDiscriminators = false,
                TypeDiscriminatorPropertyName = options.PropertyNamingPolicy?.ConvertName(PolymorphicTypeDiscriminator) ?? PolymorphicTypeDiscriminator,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };
            defaultInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(PolymorphicExample1), nameof(PolymorphicExample1)));
            defaultInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(PolymorphicExample2), nameof(PolymorphicExample2)));
        }
        else if (type == typeof(AncestorPolymorphicDto))
        {
            defaultInfo.PolymorphismOptions = new()
            {
                IgnoreUnrecognizedTypeDiscriminators = false,
                TypeDiscriminatorPropertyName = options.PropertyNamingPolicy?.ConvertName(PolymorphicTypeDiscriminator) ?? PolymorphicTypeDiscriminator,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };
            defaultInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(BasePolymorphicDto), nameof(BasePolymorphicDto)));
            defaultInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(OtherBasePolymorphicDto), nameof(OtherBasePolymorphicDto)));
        }

        return defaultInfo;
    }
}
