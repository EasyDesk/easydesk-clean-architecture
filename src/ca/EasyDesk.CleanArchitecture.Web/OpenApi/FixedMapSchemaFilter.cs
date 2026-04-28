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
        var equivalentType = typeof(IDictionary<,>).MakeGenericType(type.GetGenericArguments());
        var equivalentSchema = context.SchemaGenerator.GenerateSchema(equivalentType, context.SchemaRepository, context.MemberInfo, context.ParameterInfo);
        concreteSchema.CopyFunctionalFieldsFrom(equivalentSchema);
    }
}
