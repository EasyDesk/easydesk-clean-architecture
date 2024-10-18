using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class ErrorCodeSchemaFilter : ISchemaFilter
{
    private readonly ISerializerDataContractResolver _serializerDataContractResolver;

    public ErrorCodeSchemaFilter(ISerializerDataContractResolver serializerDataContractResolver)
    {
        _serializerDataContractResolver = serializerDataContractResolver;
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsAssignableFrom(typeof(ErrorDto)))
        {
            return;
        }
        var codeDataProperty = _serializerDataContractResolver
            .GetDataContractForType(typeof(ErrorDto))
            .ObjectProperties
            .First(p => p.MemberInfo == typeof(ErrorDto).GetProperty(nameof(ErrorDto.Code)));
        schema.Properties[codeDataProperty.Name] = new OpenApiSchema
        {
            Type = "string",
            Enum = ErrorCodes().Select(code => new OpenApiString(code) as IOpenApiAny).ToList(),
            ReadOnly = true,
        };
    }

    private IEnumerable<string> ErrorCodes() => ["a", "b"];
}
