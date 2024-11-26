using EasyDesk.SampleApp.Application.V_1_0.Dto;
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

        if (type == typeof(IPolymorphicDto))
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

        return defaultInfo;
    }
}
