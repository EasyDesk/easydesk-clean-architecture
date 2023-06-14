using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class DefaultUserPermissionsProvider : IUserPermissionsProvider
{
    private readonly IUserRolesProvider _userRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public DefaultUserPermissionsProvider(IUserRolesProvider userRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
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
