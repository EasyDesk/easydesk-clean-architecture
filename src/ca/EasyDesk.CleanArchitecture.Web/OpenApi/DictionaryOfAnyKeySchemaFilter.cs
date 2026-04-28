using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class DictionaryOfAnyKeySchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType
            || !(
                type.GetGenericTypeDefinition().IsAssignableTo(typeof(IDictionary<,>))
                    || type.GetGenericTypeDefinition().IsAssignableTo(typeof(IImmutableDictionary<,>)))
            || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var genericArguments = type.GetGenericArguments();
        if (genericArguments[0] == typeof(string) || genericArguments[0].IsEnum)
        {
            return;
        }
        var equivalentType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(genericArguments));
        var equivalentSchema = context.SchemaGenerator.GenerateSchema(equivalentType, context.SchemaRepository, context.MemberInfo, context.ParameterInfo);
        concreteSchema.CopyFunctionalFieldsFrom(equivalentSchema);
    }
}
