using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Swagger;

public class ApiVersionFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return;
        }
        var version = descriptor
            .ControllerTypeInfo
            .GetControllerVersion()
            .IfPresent(v =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "version",
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        ReadOnly = true,
                        Type = "String",
                        Default = new OpenApiString(v.ToString())
                    }
                });
            });
    }
}
