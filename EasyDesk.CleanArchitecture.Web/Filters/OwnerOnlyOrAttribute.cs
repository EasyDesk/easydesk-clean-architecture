using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Domain.Model.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OwnerOnlyOrAttribute : Attribute, IAsyncActionFilter
    {
        private const string _defaultParamName = "userId";

        private readonly IEnumerable<Role> _authorizedRoles;

        public OwnerOnlyOrAttribute(params Role[] authorizedRoles)
        {
            _authorizedRoles = authorizedRoles;
        }

        public string RouteParamName { get; set; } = _defaultParamName;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!IsAuthorized(context))
            {
                context.Result = new ForbidResult();
                return;
            }
            await next();
        }

        private bool IsAuthorized(ActionExecutingContext context)
        {
            var userInfo = context.HttpContext.RequestServices.GetRequiredService<IUserInfo>();

            if (_authorizedRoles.Intersect(userInfo.Roles).Any())
            {
                return true;
            }

            if (!context.RouteData.Values.TryGetValue(RouteParamName, out var value))
            {
                return false;
            }
            var userIdParam = Guid.Parse(value.ToString());
            return userInfo.IsLoggedIn && userInfo.UserId == userIdParam;
        }
    }
}
