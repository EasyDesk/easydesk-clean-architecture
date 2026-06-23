using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class FixedListSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!type.IsGenericType || !type.IsConstructedFrom(typeof(IFixedList<>), out var constructedType))
        {
            return;
        }
        var typeArguments = constructedType.GetGenericArguments();
        var equivalentType = typeof(IReadOnlyList<>).MakeGenericType(typeArguments);
        var equivalentSchema = await context.GetOrCreateSchemaAsync(equivalentType, context.ParameterDescription, cancellationToken);
        schema.CopyFunctionalFieldsFrom(equivalentSchema);
        schema.Metadata = null;
    }
}
