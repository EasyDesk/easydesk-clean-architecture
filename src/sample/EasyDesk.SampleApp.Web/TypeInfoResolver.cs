using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EasyDesk.SampleApp.Web;

public class TypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public const string PolymorphicTypeDiscriminator = "Discriminator";

    private static readonly IFixedMap<Type, IEnumerable<Type>> _polymorphicTypes = ImmutableCollections.Map<Type, IEnumerable<Type>>(
        (typeof(BasePolymorphicDto), [typeof(PolymorphicExample1), typeof(PolymorphicExample2),]),
        (typeof(AncestorPolymorphicDto), [typeof(BasePolymorphicDto), typeof(OtherBasePolymorphicDto),]),
        (typeof(IEmptyPolymorphicInterface), [typeof(PolymorphicExample1), typeof(PolymorphicExample2),])
    );

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var defaultInfo = base.GetTypeInfo(type, options);

        foreach (var (baseType, derivedTypes) in _polymorphicTypes)
        {
            if (baseType == type)
            {
                defaultInfo.PolymorphismOptions = new()
                {
                    IgnoreUnrecognizedTypeDiscriminators = false,
                    TypeDiscriminatorPropertyName = options.PropertyNamingPolicy?.ConvertName(PolymorphicTypeDiscriminator) ?? PolymorphicTypeDiscriminator,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };
                foreach (var derivedType in derivedTypes)
                {
                    defaultInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, derivedType.Name));
                }
            }
        }

        return defaultInfo;
    }
}
