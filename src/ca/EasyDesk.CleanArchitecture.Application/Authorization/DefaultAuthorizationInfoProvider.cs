using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationInfoProvider : IAuthorizationInfoProvider
{
    private readonly IContextProvider _contextProvider;
    private readonly IUserPermissionsProvider _userPermissionsProvider;

    public DefaultAuthorizationInfoProvider(IContextProvider contextProvider, IUserPermissionsProvider userPermissionsProvider)
    {
        _contextProvider = contextProvider;
        _userPermissionsProvider = userPermissionsProvider;
    }

    public async Task<Option<AuthorizationInfo>> GetAuthorizationInfo()
    {
        return await _contextProvider
            .GetUserInfo()
            .MapAsync(u => _userPermissionsProvider
                .GetPermissionsForUser(u)
                .Map(p => new AuthorizationInfo(u, p)));
    }
}
