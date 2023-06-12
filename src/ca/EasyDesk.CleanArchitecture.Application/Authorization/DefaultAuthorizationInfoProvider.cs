using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationInfoProvider : IAuthorizationInfoProvider
{
    private readonly IUserRolesProvider _userRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public DefaultAuthorizationInfoProvider(IUserRolesProvider userRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
    {
        _userRolesProvider = userRolesProvider;
        _rolesToPermissionsMapper = rolesToPermissionsMapper;
    }

    public async Task<AuthorizationInfo> GetAuthorizationInfoForUser(UserInfo userInfo)
    {
        var roles = await _userRolesProvider.GetRolesForUser(userInfo);
        var permissions = await _rolesToPermissionsMapper.MapRolesToPermissions(roles);
        return new(permissions);
    }
}
