using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.Commons.Collections;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

internal class DefaultStaticAuthorizer : IStaticAuthorizer
{
    public Task<bool> IsAuthorized<T>(T request, AuthorizationInfo authorizationInfo)
    {
        var requirements = typeof(T)
            .GetCustomAttributes()
            .Where(a => a.GetType().IsAssignableTo(typeof(IStaticAuthorizationRequirement)))
            .Cast<IStaticAuthorizationRequirement>();

        return Task.FromResult(requirements.All(r => r.IsSatisfied(authorizationInfo)));
    }
}
