using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class OptionSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsSubtypeOrImplementationOf(typeof(Option<>)))
        {
            return;
        }
        var wrappedType = type.GetGenericArguments()[0];
        var wrappedSchema = await context.GetOrCreateSchemaAsync(wrappedType, context.ParameterDescription, cancellationToken);
        if (wrappedSchema.Type is null || wrappedSchema.GetSchemaId() is not null)
        {
            schema.CopyFunctionalFieldsFrom(new OpenApiSchema
            {
                Type = JsonSchemaType.Null | (wrappedSchema.Type ?? JsonSchemaType.Object),
                AllOf = [wrappedSchema,],
            });
        }
        else
        {
            schema.CopyFunctionalFieldsFrom(wrappedSchema);
            schema.Type |= JsonSchemaType.Null;
        }
        schema.Metadata = null;
        return;
    }
}
