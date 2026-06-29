using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class PolymorphismSchemaAndDocumentTransformer : IOpenApiDocumentTransformer, IOpenApiSchemaTransformer
{
    public const string OneOfPolymorphicExtensionName = "x-polymorphic-oneof";

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.AnyOf is null || schema.OneOf is not null || schema.AnyOf.Count == 0 || context.JsonTypeInfo.PolymorphismOptions is null)
        {
            return Task.CompletedTask;
        }
        var derivedTypes = context.JsonTypeInfo.GetDerivedTypes()
            .Select(t => t.DerivedType)
            .ToHashSet();
        var derivedConcreteIndependentTypes = derivedTypes
            .Where(t => t.IsConcrete && !derivedTypes.Any(dt => dt != t && dt.IsAssignableTo(t)))
            .ToHashSet();

        if (derivedConcreteIndependentTypes.Count == derivedTypes.Count)
        {
            schema.Metadata ??= new Dictionary<string, object>();
            schema.Metadata[OneOfPolymorphicExtensionName] = true;
        }
        return Task.CompletedTask;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var schema in document.Components?.Schemas?.Values ?? [])
        {
            if (schema is OpenApiSchema openApiSchema
                && openApiSchema.Metadata?.TryGetValue(OneOfPolymorphicExtensionName, out var isOneOfPolymorphic) == true
                && isOneOfPolymorphic is true
                && openApiSchema.OneOf is null
                && openApiSchema.AnyOf is not null)
            {
                openApiSchema.OneOf = openApiSchema.AnyOf;
                openApiSchema.AnyOf = null;
            }
        }
        return Task.CompletedTask;
    }
}
