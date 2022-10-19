using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public class AddApiVersionParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return;
        }
        descriptor
            .ControllerTypeInfo
            .GetControllerVersion()
            .IfPresent(v => AddApiVersionParameter(operation, v));
    }

    private static void AddApiVersionParameter(OpenApiOperation operation, ApiVersion version)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "version",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema
            {
                ReadOnly = true,
                Type = "string",
                Default = new OpenApiString(version.ToString())
            }
        });
    }
}
