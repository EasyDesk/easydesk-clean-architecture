using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class CollectionsOfCustomTypesSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if ((schema.Type & JsonSchemaType.Object) == JsonSchemaType.Object
            && schema.AdditionalPropertiesAllowed
            && schema.AdditionalProperties is null
            && (type.IsConstructedFrom(typeof(IDictionary<,>), out var constructedType) || type.IsConstructedFrom(typeof(IReadOnlyDictionary<,>), out constructedType)))
        {
            var valueType = constructedType.GetGenericArguments()[1];
            schema.AdditionalProperties ??= await context.GetOrCreateSchemaAsync(valueType, context.ParameterDescription, cancellationToken);
            schema.Metadata = null;
        }
        else if ((schema.Type & JsonSchemaType.Array) == JsonSchemaType.Array && type.IsConstructedFrom(typeof(IEnumerable<>), out constructedType))
        {
            var itemType = constructedType.GetGenericArguments()[0];
            schema.Items ??= await context.GetOrCreateSchemaAsync(itemType, context.ParameterDescription, cancellationToken);
            schema.Metadata = null;
        }
    }
}
