using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class InheritanceSchemaAndDocumentTransformer : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    public const string DerivedSchemaExtensionName = "x-inheritance-allof";

    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Document is null || schema.AnyOf is null || schema.AnyOf.Count == 0 || context.JsonTypeInfo.PolymorphismOptions is null)
        {
            return;
        }
        schema.Discriminator ??= new();
        schema.Discriminator.PropertyName ??= context.JsonTypeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName;
        var parentProperties = context.JsonTypeInfo.Properties
            .Select(p => context.JsonTypeInfo.Options.PropertyNamingPolicy?.ConvertName(p.Name) ?? p.Name)
            .ToFixedList();
        // Copy properties from children to parent if they are not already present in the parent, and mark them as required if they are required
        var firstChild = schema.AnyOf[0];
        foreach (var parentProperty in parentProperties)
        {
            if (schema.Properties?.ContainsKey(parentProperty) != true && firstChild.Properties?.ContainsKey(parentProperty) == true)
            {
                schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
                schema.Properties[parentProperty] = firstChild.Properties[parentProperty];
            }
            if (firstChild.Required?.Contains(parentProperty) == true)
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(parentProperty);
            }
        }
        schema.AnyOf = null;
        schema.OneOf = null;
        schema.Discriminator.Mapping = null;
        foreach (var derivedType in context.JsonTypeInfo.GetDerivedTypes())
        {
            if (derivedType.DerivedType == context.JsonTypeInfo.Type)
            {
                schema.Required?.Remove(schema.Discriminator.PropertyName);
            }
            else
            {
                var derivedTypeInfo = context.JsonTypeInfo.Options.GetTypeInfo(derivedType.DerivedType);
                var derivedSchema = await context.GetOrCreateSchemaAsync(derivedType.DerivedType, null, cancellationToken);
                var derivedId = derivedSchema.GetSchemaId() ?? derivedTypeInfo.Type.Name;
                if (context.Document.Components?.Schemas?.TryGetValue(derivedId, out var registeredDerivedSchema) == true && registeredDerivedSchema is OpenApiSchema registeredDerivedConcreteSchema)
                {
                    derivedSchema = registeredDerivedConcreteSchema;
                }
                else
                {
                    context.Document.AddComponent(derivedId, derivedSchema);
                }
                ConfigureInheritanceWithAllOf(schema, derivedSchema, context.Document);
                schema.Discriminator.Mapping ??= new Dictionary<string, OpenApiSchemaReference>();
                schema.Discriminator.Mapping[$"{derivedType.TypeDiscriminator}"] = new(derivedId, context.Document);
            }
        }
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var (parentSchemaId, parentSchema) in document.Components?.Schemas?.AsEnumerable() ?? [])
        {
            var compatibleChildren = parentSchema.OneOf
                ?.Select(s => (s is OpenApiSchemaReference r ? r.Target : s) as OpenApiSchema)
                .ZipWithIndex()
                .Where(s => s.Item is not null && s.Item.Metadata?.TryGetValue(DerivedSchemaExtensionName, out var v) == true && v is true)
                .ToList()
                ?? [];
            compatibleChildren.Reverse();
            foreach (var (derivedSchema, index) in compatibleChildren)
            {
                derivedSchema!.AllOf ??= [];
                if (!derivedSchema.AllOf.Any(s => s.GetSchemaId() == parentSchemaId))
                {
                    derivedSchema.AllOf.Add(new OpenApiSchemaReference(parentSchemaId, document));
                }
                parentSchema.OneOf!.RemoveAt(index);
            }
        }
        return Task.CompletedTask;
    }

    private void ConfigureInheritanceWithAllOf(OpenApiSchema parentSchema, OpenApiSchema derivedSchema, OpenApiDocument document)
    {
        derivedSchema.AllOf ??= [];
        var allOfBackup = derivedSchema.AllOf;
        var oneOfBackup = derivedSchema.OneOf;
        var anyofBackup = derivedSchema.AnyOf;
        var metadataBackup = derivedSchema.Metadata;
        var discriminatorBackup = derivedSchema.Discriminator;
        OpenApiSchema proxySchema;
        if (allOfBackup.Count > 0 && allOfBackup[0] is OpenApiSchema cs && cs.Metadata?.TryGetValue(DerivedSchemaExtensionName, out var v) == true && v is true)
        {
            proxySchema = cs;
        }
        else
        {
            proxySchema = new();
            proxySchema.CopyFunctionalFieldsFrom(derivedSchema);
            proxySchema.AllOf = null;
            proxySchema.OneOf = null;
            proxySchema.AnyOf = null;
            proxySchema.Discriminator = null;
            proxySchema.Metadata = new Dictionary<string, object>
            {
                [DerivedSchemaExtensionName] = true,
            };
            allOfBackup.Insert(0, proxySchema);
            derivedSchema.ResetFunctionalFields();
            derivedSchema.AllOf = allOfBackup;
            derivedSchema.OneOf = oneOfBackup;
            derivedSchema.AnyOf = anyofBackup;
            derivedSchema.Metadata = metadataBackup;
            derivedSchema.Discriminator = discriminatorBackup;
        }
        RemoveRedundantProperties(parentSchema, proxySchema);
        // We need to append the derivedSchema to the parentSchema in order to resolve its references.
        // Later, we will re-invert the dependency and remove the derivedSchema from the parentSchema,
        // adding the parentSchema as allOf to the derivedSchema, so that the derivedSchema will have all the properties of the parentSchema.
        parentSchema.OneOf ??= [];
        parentSchema.OneOf.Add(derivedSchema);
        // We are also marking the derived schema to avoid clashing with other one ofs
        derivedSchema.Metadata ??= new Dictionary<string, object>();
        derivedSchema.Metadata[DerivedSchemaExtensionName] = true;
    }

    private void RemoveRedundantProperties(IOpenApiSchema parentSchema, OpenApiSchema derivedSchema)
    {
        foreach (var parentProperty in parentSchema.Properties?.Keys ?? [])
        {
            derivedSchema.Properties?.Remove(parentProperty);
            derivedSchema.Required?.Remove(parentProperty);
        }
        if (derivedSchema.Properties is { Count: 0, })
        {
            derivedSchema.Properties = null;
        }
        if (derivedSchema.Required is { Count: 0, })
        {
            derivedSchema.Required = null;
        }
    }
}
