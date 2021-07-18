using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.Filters
{
    public class DisabledEndpointsFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (IsDisabled(context))
            {
                context.Result = new NotFoundResult();
            }
        }

        private bool IsDisabled(AuthorizationFilterContext context)
        {
            var disabledEndpoints = context.HttpContext.RequestServices.GetRequiredService<DisabledEndpoints>();
            var descriptor = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (descriptor != null)
            {
                var controllerMatches = disabledEndpoints.Controllers?.Contains(descriptor.ControllerName) ?? false;
                var actionMatches = disabledEndpoints.Actions?.Contains($"{descriptor.ControllerName}.{descriptor.ActionName}") ?? false;
                return controllerMatches || actionMatches;
            }
            return false;
        }
    }

    public class DisabledEndpoints
    {
        public IEnumerable<string> Controllers { get; set; }

        public IEnumerable<string> Actions { get; set; }
    }
}
