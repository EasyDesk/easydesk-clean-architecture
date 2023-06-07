using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class TenantIdOperationFilterForDefaultContextReader : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType == typeof(ApiVersioningController))
        {
            return;
        }
        operation.Parameters ??= new List<OpenApiParameter>();
        if (!operation.Parameters.Any(p => p.Name.Equals(MultitenancyDefaults.TenantIdHttpQueryParam)))
        {
            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = MultitenancyDefaults.TenantIdHttpQueryParam,
                In = ParameterLocation.Query,
                Description = $"Optional parameter alternative to setting the {MultitenancyDefaults.TenantIdHttpHeader} header for specifying the tenant id of the request, with lower priority than the header.",
                Required = false,
                AllowEmptyValue = true,
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                    MaxLength = TenantId.MaxLength,
                }
            });
        }
    }
}
