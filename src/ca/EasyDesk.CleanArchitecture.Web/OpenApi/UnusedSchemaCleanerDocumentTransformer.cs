using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class UnusedSchemaCleanerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var visitor = new OpenApiReferenceVisitor();
        var walker = new OpenApiWalker(visitor);
        walker.Walk(document);

        var unusedSchemaNames = new HashSet<string>();

        foreach (var schemaId in document
            .Components
            ?.Schemas
            ?.Select(schema => schema.Key)
            ?.Where(schemaId => !visitor.UsedSchemaNames.Contains(schemaId))
            ?? [])
        {
            unusedSchemaNames.Add(schemaId);
        }

        foreach (var schemaId in unusedSchemaNames)
        {
            document.Components?.Schemas?.Remove(schemaId);
        }
        return Task.CompletedTask;
    }

    private sealed class OpenApiReferenceVisitor : OpenApiVisitorBase
    {
        public HashSet<string> UsedSchemaNames { get; } = [];

        public override void Visit(IOpenApiReferenceHolder referenceHolder)
        {
            var type = referenceHolder.GetType();
            if (type.GetInterfaces()
                .FirstOption(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IOpenApiReferenceHolder<>))
                .Filter(i => i.GetGenericArguments()[0].IsSubclassOf(typeof(BaseOpenApiReference)))
                .IsPresent(out var genericInterface))
            {
                var reference = (BaseOpenApiReference)type
                    .GetProperty(nameof(IOpenApiReferenceHolder<>.Reference))!
                    .GetValue(referenceHolder)!;
                if (reference.Type is ReferenceType.Schema && reference.Id is not null)
                {
                    UsedSchemaNames.Add(reference.Id);
                }
            }
        }

        public override void Visit(IOpenApiSchema schema)
        {
            if (schema.Discriminator is not null)
            {
                foreach (var reference in schema.Discriminator.Mapping?.Values?.Select(r => r.Reference) ?? [])
                {
                    if (reference.Type is ReferenceType.Schema && reference.Id is not null)
                    {
                        UsedSchemaNames.Add(reference.Id);
                    }
                }
            }
        }
    }
}
