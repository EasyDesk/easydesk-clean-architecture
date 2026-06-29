using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static class OpenApiSchemaExtensions
{
    private const string SchemaIdMetadataKey = "x-schema-id";

    public static void CopyFunctionalFieldsFrom(this OpenApiSchema to, IOpenApiSchema from)
    {
        to.Type = from.Type;
        to.Format = from.Format;
        to.Description = from.Description;
        to.Default = from.Default;
        to.ReadOnly = from.ReadOnly;
        to.WriteOnly = from.WriteOnly;
        to.AllOf = from.AllOf?.ToList();
        to.OneOf = from.OneOf?.ToList();
        to.AnyOf = from.AnyOf?.ToList();
        to.Not = from.Not;
        to.Required = from.Required?.ToHashSet();
        to.Items = from.Items;
        to.UniqueItems = from.UniqueItems;
        to.Properties = from.Properties?.ToDictionary();
        to.MaxProperties = from.MaxProperties;
        to.MinProperties = from.MinProperties;
        to.AdditionalPropertiesAllowed = from.AdditionalPropertiesAllowed;
        to.AdditionalProperties = from.AdditionalProperties;
        to.Discriminator = from.Discriminator is not null
            ? new OpenApiDiscriminator
            {
                PropertyName = from.Discriminator.PropertyName,
                Mapping = from.Discriminator.Mapping?.ToDictionary(),
            }
            : null;
        to.Example = from.Example;
        to.Enum = from.Enum?.ToList();
        to.ExternalDocs = from.ExternalDocs;
        to.Deprecated = from.Deprecated;
        to.Xml = from.Xml;
        to.Extensions = from.Extensions?.ToDictionary();
        to.Const = from.Const;
    }

    public static void ResetFunctionalFields(this OpenApiSchema schema)
    {
        schema.CopyFunctionalFieldsFrom(new OpenApiSchema());
    }

    public static OpenApiSchema ResolveReference(this IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema concreteSchema)
        {
            return concreteSchema;
        }
        else if (schema is OpenApiSchemaReference reference && reference.Target is not null)
        {
            return reference.Target.ResolveReference();
        }
        else
        {
            throw new InvalidOperationException($"Cannot resolve reference for schema of type {schema.GetType().Name}");
        }
    }

    public static string? GetDerivedSchemaId(this IOpenApiSchema schema, string baseSchemaId)
    {
        var schemaId = schema.GetSchemaId();
        if (schemaId is null)
        {
            return null;
        }
        return $"{baseSchemaId}{schemaId}";
    }

    public static string? GetSchemaId(this IOpenApiSchema schema)
    {
        if (schema.ResolveReference().Metadata?.TryGetValue(SchemaIdMetadataKey, out var id) == true)
        {
            var idString = id?.ToString();
            return idString?.Length == 0 ? null : idString;
        }
        return null;
    }

    public static void SetSchemaId(this IOpenApiSchema schema, string? id)
    {
        var resolvedSchema = schema.ResolveReference();
        if (resolvedSchema.Metadata is null)
        {
            resolvedSchema.Metadata = new Dictionary<string, object>();
        }
        if (id is null)
        {
            resolvedSchema.Metadata.Remove(SchemaIdMetadataKey);
        }
        else
        {
            resolvedSchema.Metadata[SchemaIdMetadataKey] = id;
        }
    }
}
