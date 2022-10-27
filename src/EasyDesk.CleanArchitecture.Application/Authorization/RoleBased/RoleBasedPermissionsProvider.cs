using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

internal class RoleBasedPermissionsProvider : IPermissionsProvider
{
    private readonly IUserRolesProvider _userRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public RoleBasedPermissionsProvider(IUserRolesProvider userRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
    {
        _userRolesProvider = userRolesProvider;
        _rolesToPermissionsMapper = rolesToPermissionsMapper;
    }

    public async Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userInfo)
    {
        var roles = await _userRolesProvider.GetRolesForUser(userInfo);
        return await _rolesToPermissionsMapper.MapRolesToPermissions(roles);
    }
}
