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
        to.AllOf = from.AllOf;
        to.OneOf = from.OneOf;
        to.AnyOf = from.AnyOf;
        to.Not = from.Not;
        to.Required = from.Required;
        to.Items = from.Items;
        to.UniqueItems = from.UniqueItems;
        to.Properties = from.Properties;
        to.MaxProperties = from.MaxProperties;
        to.MinProperties = from.MinProperties;
        to.AdditionalPropertiesAllowed = from.AdditionalPropertiesAllowed;
        to.AdditionalProperties = from.AdditionalProperties;
        to.Discriminator = from.Discriminator;
        to.Example = from.Example;
        to.Enum = from.Enum;
        to.ExternalDocs = from.ExternalDocs;
        to.Deprecated = from.Deprecated;
        to.Xml = from.Xml;
        to.Extensions = from.Extensions;
        to.Const = from.Const;
    }
}
