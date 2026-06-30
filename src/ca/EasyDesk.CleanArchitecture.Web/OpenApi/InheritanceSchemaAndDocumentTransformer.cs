using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Concurrent;

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
        var schemaId = schema.GetSchemaId();
        if (schemaId is null)
        {
            return;
        }
        schema.Metadata?[DerivedSchemaExtensionName] = schemaId;
        schema.Discriminator ??= new();
        schema.Discriminator.PropertyName ??= context.JsonTypeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName;
        if (schema.Discriminator.Mapping is null)
        {
            schema.Discriminator.Mapping = new Dictionary<string, OpenApiSchemaReference>();
            foreach (var derivedType in context.JsonTypeInfo.PolymorphismOptions.DerivedTypes)
            {
                if (derivedType.TypeDiscriminator is null)
                {
                    throw new InvalidOperationException($"Type discriminator is null for derived type {derivedType.DerivedType.FullName} of base type {context.JsonTypeInfo.Type.FullName}.");
                }
                var derivedSchema = await context.GetOrCreateSchemaAsync(derivedType.DerivedType, null, cancellationToken);
                var derivedSchemaId = derivedSchema.GetDerivedSchemaId(schemaId);
                if (derivedSchemaId is not null)
                {
                    schema.Discriminator.Mapping[$"{derivedType.TypeDiscriminator}"] = new(derivedSchemaId, context.Document);
                }
            }
        }
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
        foreach (var (derivedSchema, index) in schema.AnyOf.ZipWithIndex())
        {
            var derivedId = derivedSchema.GetSchemaId();
            if (derivedId is null)
            {
                continue;
            }
            var concreteDerivedSchema = derivedSchema.ResolveReference();
            concreteDerivedSchema.Type ??= schema.Type;
            if (derivedId == "Base" || derivedId == schemaId)
            {
                schema.Required?.Remove(schema.Discriminator.PropertyName);
                derivedId = schemaId;
                concreteDerivedSchema = schema;
            }
            concreteDerivedSchema.Metadata ??= new Dictionary<string, object>();
            concreteDerivedSchema.Metadata[DerivedSchemaExtensionName] = derivedId;
            concreteDerivedSchema.Properties?.Remove(schema.Discriminator.PropertyName);
            concreteDerivedSchema.Required?.Remove(schema.Discriminator.PropertyName);
        }
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is null)
        {
            return Task.CompletedTask;
        }
        document.Components.Schemas = new ConcurrentDictionary<string, IOpenApiSchema>(document.Components.Schemas);
        foreach (var (parentSchemaId, parentSchema) in document.Components.Schemas)
        {
            var concreteParentSchema = parentSchema.ResolveReference();
            if (parentSchema.Discriminator?.Mapping is null || concreteParentSchema.Metadata?.GetOption(DerivedSchemaExtensionName).OrElseNull() is not string)
            {
                continue;
            }
            var oldMappings = parentSchema.Discriminator.Mapping.Select(kvp => (kvp.Key, kvp.Value.Reference.HostDocument is null ? new(kvp.Value.Reference.Id!, document) : kvp.Value)).ToList();
            parentSchema.Discriminator.Mapping.Clear();
            foreach (var (key, reference) in oldMappings)
            {
                var referenced = reference.ResolveReference();
                var derivedId = referenced.Metadata?.GetOption(DerivedSchemaExtensionName).OrElseNull() as string ?? referenced.GetSchemaId();
                if (derivedId is null || derivedId == parentSchemaId)
                {
                    continue;
                }
                if (reference.Reference.Id is not null && reference.Reference.Id != derivedId)
                {
                    document.Components.Schemas.Remove(reference.Reference.Id);
                }
                parentSchema.Discriminator.Mapping.Add(key, new(derivedId, document));
                OpenApiSchema derivedSchema;
                if (document.Components.Schemas.TryGetValue(derivedId, out var derivedSchemaRef))
                {
                    derivedSchema = derivedSchemaRef.ResolveReference();
                }
                else
                {
                    derivedSchema = referenced.CreateShallowCopy().ResolveReference();
                    derivedSchema.SetSchemaId(derivedId);
                    document.AddComponent(derivedId, derivedSchema);
                }
                derivedSchema.AllOf ??= [];
                if (!derivedSchema.AllOf.Any(s => s.GetSchemaId() == parentSchemaId))
                {
                    derivedSchema.AllOf.Add(new OpenApiSchemaReference(parentSchemaId, document));
                }
            }
            concreteParentSchema.AnyOf = null;
        }
        foreach (var mixedSchema in document.Components?.Schemas?.Values?.Select(s => s.ResolveReference()) ?? [])
        {
            if (mixedSchema.AllOf is { Count: > 0, } && mixedSchema.Properties?.Values is { Count: > 0, })
            {
                var parentSchemas = mixedSchema.AllOf.Select(s => s.ResolveReference()).ToList();
                foreach (var parentSchema in parentSchemas)
                {
                    ConfigureInheritanceWithAllOf(parentSchema, mixedSchema, document);
                }
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
    }

    private void RemoveRedundantProperties(IOpenApiSchema parentSchema, OpenApiSchema derivedSchema)
    {
        foreach (var parentProperty in InheritedProperties(parentSchema).ToFixedSortedSet())
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

    private static IEnumerable<string> InheritedProperties(IOpenApiSchema schema)
    {
        foreach (var parentSchema in schema.AllOf?.Select(s => s.ResolveReference()) ?? [])
        {
            foreach (var parentProperty in InheritedProperties(parentSchema))
            {
                yield return parentProperty;
            }
        }
        foreach (var property in schema.Properties?.Keys ?? [])
        {
            yield return property;
        }
    }
}
