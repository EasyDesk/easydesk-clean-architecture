using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class InheritanceSchemaTransformer : IOpenApiSchemaTransformer
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

    private void ConfigureInheritanceWithAllOf(OpenApiSchema parentSchema, OpenApiSchema derivedSchema, OpenApiDocument document)
    {
        RemoveRedundantProperties(parentSchema, derivedSchema);
        derivedSchema.AllOf ??= [];
        var proxySchema = derivedSchema.AllOf.SelectMany(s => (s as OpenApiSchema).AsOption()).FirstOption(s => s.Metadata?.TryGetValue(DerivedSchemaExtensionName, out var v) == true && v is true).OrElseGet(() =>
        {
            var newSchema = new OpenApiSchema
            {
                Metadata = new Dictionary<string, object>
                {
                    [DerivedSchemaExtensionName] = true,
                },
            };
            newSchema.CopyFunctionalFieldsFrom(derivedSchema);
            newSchema.Discriminator = null;
            derivedSchema.AllOf.Add(newSchema);
            return newSchema;
        });
        RemoveRedundantProperties(proxySchema, derivedSchema);
        var parentSchemaId = parentSchema.GetSchemaId() ?? throw new InvalidOperationException("Parent schema must have an ID");
        if (!derivedSchema.AllOf.Any(s => s.GetSchemaId() == parentSchemaId))
        {
            derivedSchema.AllOf.Add(new OpenApiSchemaReference(parentSchemaId, document));
        }
    }

    private void RemoveRedundantProperties(IOpenApiSchema parentSchema, IOpenApiSchema derivedSchema)
    {
        foreach (var parentProperty in parentSchema.Properties?.Keys ?? [])
        {
            derivedSchema.Properties?.Remove(parentProperty);
            derivedSchema.Required?.Remove(parentProperty);
        }
    }
}
