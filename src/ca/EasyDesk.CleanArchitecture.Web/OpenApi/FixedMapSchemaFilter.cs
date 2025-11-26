using EasyDesk.Commons.Collections.Immutable;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class FixedMapSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsAssignableTo(typeof(IFixedMap<,>)) || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var equivalentType = context.SchemaGenerator
            .GenerateSchema(typeof(IDictionary<,>).MakeGenericType(type.GetGenericArguments()), context.SchemaRepository, context.MemberInfo, context.ParameterInfo);
        concreteSchema.Type = equivalentType.Type;
        concreteSchema.Format = equivalentType.Format;
        concreteSchema.Description = equivalentType.Description;
        concreteSchema.Default = equivalentType.Default;
        concreteSchema.ReadOnly = equivalentType.ReadOnly;
        concreteSchema.WriteOnly = equivalentType.WriteOnly;
        concreteSchema.AllOf = equivalentType.AllOf;
        concreteSchema.OneOf = equivalentType.OneOf;
        concreteSchema.AnyOf = equivalentType.AnyOf;
        concreteSchema.Not = equivalentType.Not;
        concreteSchema.Required = equivalentType.Required;
        concreteSchema.Items = equivalentType.Items;
        concreteSchema.UniqueItems = equivalentType.UniqueItems;
        concreteSchema.Properties = equivalentType.Properties;
        concreteSchema.MaxProperties = equivalentType.MaxProperties;
        concreteSchema.MinProperties = equivalentType.MinProperties;
        concreteSchema.AdditionalPropertiesAllowed = equivalentType.AdditionalPropertiesAllowed;
        concreteSchema.AdditionalProperties = equivalentType.AdditionalProperties;
        concreteSchema.Discriminator = equivalentType.Discriminator;
        concreteSchema.Example = equivalentType.Example;
        concreteSchema.Enum = equivalentType.Enum;
        concreteSchema.ExternalDocs = equivalentType.ExternalDocs;
        concreteSchema.Deprecated = equivalentType.Deprecated;
        concreteSchema.Xml = equivalentType.Xml;
        concreteSchema.Extensions = equivalentType.Extensions;
        concreteSchema.Const = equivalentType.Const;
    }
}
