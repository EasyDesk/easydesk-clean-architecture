using EasyDesk.Commons.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class OptionSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsAssignableTo(typeof(Option<>)) || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var wrappedType = type.GetGenericArguments()[0];
        var wrappedSchema = context.SchemaGenerator.GenerateSchema(wrappedType, context.SchemaRepository);
        concreteSchema.Type = wrappedSchema.Type | JsonSchemaType.Null;
        concreteSchema.Default = wrappedSchema.Default;
        concreteSchema.AllOf = [wrappedSchema,];
        concreteSchema.Deprecated = wrappedSchema.Deprecated;
        concreteSchema.Description = wrappedSchema.Description;
        concreteSchema.Example = concreteSchema.Example;
        concreteSchema.Items = wrappedSchema.Items;
    }
}
