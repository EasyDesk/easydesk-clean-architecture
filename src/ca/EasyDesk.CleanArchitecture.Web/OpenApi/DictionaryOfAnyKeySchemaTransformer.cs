using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class DictionaryOfAnyKeySchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!type.IsGenericType || !(type.IsConstructedFrom(typeof(IDictionary<,>), out var constructedType) || type.IsConstructedFrom(typeof(IReadOnlyDictionary<,>), out constructedType)))
        {
            return;
        }
        var genericArguments = constructedType.GetGenericArguments();
        if (genericArguments[0] == typeof(string) || genericArguments[0].IsEnum)
        {
            return;
        }
        var equivalentType = typeof(IList<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(genericArguments));
        var equivalentSchema = await context.GetOrCreateSchemaAsync(equivalentType, context.ParameterDescription, cancellationToken);
        schema.CopyFunctionalFieldsFrom(equivalentSchema);
        schema.Metadata = null;
    }
}
