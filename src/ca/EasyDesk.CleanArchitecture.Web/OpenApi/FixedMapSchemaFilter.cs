using EasyDesk.Commons.Collections.Immutable;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class FixedMapSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.IsConstructedFrom(typeof(IFixedMap<,>), out var constructedType) || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var typeArguments = constructedType.GetGenericArguments();
        var equivalentType = typeof(IImmutableDictionary<,>).MakeGenericType(typeArguments);
        var equivalentSchema = context.SchemaGenerator.GenerateSchema(equivalentType, context.SchemaRepository, context.MemberInfo, context.ParameterInfo);
        concreteSchema.CopyFunctionalFieldsFrom(equivalentSchema);
    }
}
