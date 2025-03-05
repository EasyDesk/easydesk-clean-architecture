using EasyDesk.Commons.Collections.Immutable;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class FixedMapSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsAssignableTo(typeof(IFixedMap<,>)))
        {
            return;
        }
        var equivalentType = context.SchemaGenerator
            .GenerateSchema(typeof(IDictionary<,>).MakeGenericType(type.GetGenericArguments()), context.SchemaRepository);
        schema.Type = equivalentType.Type;
        schema.Format = equivalentType.Format;
        schema.Description = equivalentType.Description;
        schema.Default = equivalentType.Default;
        schema.ReadOnly = equivalentType.ReadOnly;
        schema.WriteOnly = equivalentType.WriteOnly;
        schema.AllOf = equivalentType.AllOf;
        schema.OneOf = equivalentType.OneOf;
        schema.AnyOf = equivalentType.AnyOf;
        schema.Not = equivalentType.Not;
        schema.Required = equivalentType.Required;
        schema.Items = equivalentType.Items;
        schema.UniqueItems = equivalentType.UniqueItems;
        schema.Properties = equivalentType.Properties;
        schema.MaxProperties = equivalentType.MaxProperties;
        schema.MinProperties = equivalentType.MinProperties;
        schema.AdditionalPropertiesAllowed = equivalentType.AdditionalPropertiesAllowed;
        schema.AdditionalProperties = equivalentType.AdditionalProperties;
        schema.Discriminator = equivalentType.Discriminator;
        schema.Example = equivalentType.Example;
        schema.Enum = equivalentType.Enum;
        schema.Nullable = equivalentType.Nullable;
        schema.ExternalDocs = equivalentType.ExternalDocs;
        schema.Deprecated = equivalentType.Deprecated;
        schema.Xml = equivalentType.Xml;
        schema.Extensions = equivalentType.Extensions;
        schema.UnresolvedReference = equivalentType.UnresolvedReference;
        schema.Reference = equivalentType.Reference;
        schema.Annotations = equivalentType.Annotations;
    }
}
