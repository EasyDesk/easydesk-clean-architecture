using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class OptionSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsSubtypeOrImplementationOf(typeof(Option<>)) || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var wrappedType = type.GetGenericArguments()[0];
        var wrappedSchema = context.SchemaGenerator.GenerateSchema(wrappedType, context.SchemaRepository);
        concreteSchema.CopyFunctionalFieldsFrom(wrappedSchema);
        concreteSchema.Type = JsonSchemaType.Null | JsonSchemaType.Object;
        concreteSchema.AllOf = [wrappedSchema,];
    }
}
