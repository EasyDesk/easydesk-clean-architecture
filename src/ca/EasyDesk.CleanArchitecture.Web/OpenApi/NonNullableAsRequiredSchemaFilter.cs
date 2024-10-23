using EasyDesk.Commons.Collections;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class NonNullableAsRequiredSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Required = schema.Properties
            .Where(x => !x.Value.Nullable)
            .Select(x => x.Key)
            .ToHashSet();
    }
}
