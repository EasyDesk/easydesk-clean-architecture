using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static class OpenApiSchemaExtensions
{
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

    public static string? GetSchemaId(this IOpenApiSchema schema)
    {
        var concreteSchema = schema as OpenApiSchema;
        if (concreteSchema is null && schema is OpenApiSchemaReference r)
        {
            concreteSchema = r.Target as OpenApiSchema;
        }
        if (concreteSchema?.Metadata?.TryGetValue("x-schema-id", out var id) == true)
        {
            var idString = id?.ToString();
            return idString?.Length == 0 ? null : idString;
        }
        return null;
    }
}
