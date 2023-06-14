using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationInfoProvider : IAuthorizationInfoProvider
{
    private readonly IContextProvider _contextProvider;
    private readonly IIdentityPermissionsProvider _identityPermissionsProvider;

    public DefaultAuthorizationInfoProvider(IContextProvider contextProvider, IIdentityPermissionsProvider identityPermissionsProvider)
    {
        _contextProvider = contextProvider;
        _identityPermissionsProvider = identityPermissionsProvider;
    }

    public async Task<Option<AuthorizationInfo>> GetAuthorizationInfo()
    {
        return await _contextProvider
            .GetIdentity()
            .MapAsync(u => _identityPermissionsProvider
                .GetPermissionsForIdentity(u)
                .Map(p => new AuthorizationInfo(u, p)));
    }
}
