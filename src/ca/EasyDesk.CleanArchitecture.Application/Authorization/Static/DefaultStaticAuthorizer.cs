using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

internal class DefaultStaticAuthorizer : IStaticAuthorizer
{
    private readonly IAuthorizationInfoProvider _permissionsProvider;

    public DefaultStaticAuthorizer(IAuthorizationInfoProvider permissionsProvider)
    {
        _permissionsProvider = permissionsProvider;
    }

    public async Task<bool> IsAuthorized<T>(T request, UserInfo userInfo)
    {
        var requirements = typeof(T)
            .GetCustomAttributes()
            .Where(a => a.GetType().IsAssignableTo(typeof(IStaticAuthorizationRequirement)))
            .Cast<IStaticAuthorizationRequirement>();

        if (requirements.IsEmpty())
        {
            return true;
        }

        var authInfo = await _permissionsProvider.GetAuthorizationInfoForUser(userInfo);
        return requirements.All(r => r.IsSatisfied(userInfo, authInfo));
    }
}
