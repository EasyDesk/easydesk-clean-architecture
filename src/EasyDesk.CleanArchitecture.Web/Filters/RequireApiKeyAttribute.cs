using EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredName;

        public RequireApiKeyAttribute(string requiredName)
        {
            _requiredName = requiredName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.User
                .FindFirstValue(ApiKeyAuthenticationOptions.ApiKeyClaimName)
                .AsOption()
                .Filter(name => name == _requiredName)
                .IfAbsent(() => context.Result = new ForbidResult());
        }
    }
}
