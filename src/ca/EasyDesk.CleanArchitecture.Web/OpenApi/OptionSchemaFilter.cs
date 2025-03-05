using EasyDesk.Commons.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class OptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsGenericType || !type.GetGenericTypeDefinition().IsAssignableTo(typeof(Option<>)))
        {
            return;
        }
        var wrappedType = type.GetGenericArguments()[0];
        var wrappedSchema = context.SchemaGenerator.GenerateSchema(wrappedType, context.SchemaRepository);
        schema.Type = wrappedSchema.Type;
        schema.Nullable = true;
        schema.Default = wrappedSchema.Default;
        schema.AllOf.Add(wrappedSchema);
        schema.Deprecated = wrappedSchema.Deprecated;
        schema.Description = wrappedSchema.Description;
        schema.Example = schema.Example;
        schema.Items = wrappedSchema.Items;
    }
}
