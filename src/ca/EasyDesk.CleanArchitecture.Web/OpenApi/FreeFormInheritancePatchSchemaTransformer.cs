using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

/// <summary>
/// Due to https://github.com/OpenAPITools/openapi-generator/issues/7638
/// inheriting from a free-form object (object with no properties) is not supported by the OpenAPI generator
/// so we need to revert the inheritance to a an anyOf relationship.
///
/// This transformer needs to run after the document has been generated,
/// otherwise the anyOf references will be overwritten by the OpenApi library.
/// </summary>
internal class FreeFormInheritancePatchSchemaTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var schema in document.Components?.Schemas?.Values ?? [])
        {
            if (schema is not OpenApiSchema concreteSchema)
            {
                continue;
            }
            var schemaId = concreteSchema.GetSchemaId();
            if (schemaId is null || concreteSchema.AllOf is null)
            {
                continue;
            }
            for (var i = concreteSchema.AllOf.Count - 1; i >= 0; i--)
            {
                var parentSchema = concreteSchema.AllOf[i];
                if (parentSchema.Properties is { Count: > 0, } || parentSchema.AllOf is { Count: > 0, })
                {
                    continue;
                }
                if (parentSchema is OpenApiSchemaReference parentReference && parentReference.Target is OpenApiSchema parentConcrete)
                {
                    parentConcrete.OneOf ??= [];
                    parentConcrete.OneOf.Add(new OpenApiSchemaReference(schemaId, document));
                    concreteSchema.AllOf.RemoveAt(i);
                }
            }
            if (concreteSchema.AllOf.Count == 0)
            {
                concreteSchema.AllOf = null;
            }
        }
        return Task.CompletedTask;
    }
}
