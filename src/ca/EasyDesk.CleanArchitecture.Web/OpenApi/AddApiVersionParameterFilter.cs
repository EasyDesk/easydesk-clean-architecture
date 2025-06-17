using EasyDesk.CleanArchitecture.Application.Versioning;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class AddApiVersionParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        context
            .MethodInfo
            .DeclaringType
            ?.GetApiVersionFromNamespace()
            .IfPresent(v => AddApiVersionParameter(operation, v));
    }

    private static void AddApiVersionParameter(OpenApiOperation operation, ApiVersion version)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "version",
            In = ParameterLocation.Query,
            Description = "Optional parameter used to select the API version for this endpoint.",
            Schema = new()
            {
                ReadOnly = true,
                Type = "string",
                Default = new OpenApiString(version.ToStringWithoutV()),
            },
        });
    }
}
