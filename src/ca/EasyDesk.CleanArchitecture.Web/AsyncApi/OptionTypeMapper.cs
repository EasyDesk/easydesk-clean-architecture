using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;

namespace EasyDesk.CleanArchitecture.Web.AsyncApi;

internal class OptionTypeMapper : ITypeMapper
{
    public Type MappedType => typeof(Option<>);

    public bool UseReference => false;

    public void GenerateSchema(JsonSchema schema, TypeMapperContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Option<>)))
        {
            return;
        }
        var wrappedType = type.GetGenericArguments()[0];
        context.JsonSchemaGenerator.Generate(schema, wrappedType, context.JsonSchemaResolver);
        schema.IsNullableRaw = true;
    }
}
