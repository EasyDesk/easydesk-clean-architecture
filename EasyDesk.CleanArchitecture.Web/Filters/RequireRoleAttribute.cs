using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Domain.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly RoleId[] _roles;

        public RequireRoleAttribute(params RoleId[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userInfoRetriever = context.HttpContext.RequestServices.GetRequiredService<IUserInfo>();

            var authorized = userInfoRetriever.Roles.Intersect(_roles).Any();

            if (!authorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
