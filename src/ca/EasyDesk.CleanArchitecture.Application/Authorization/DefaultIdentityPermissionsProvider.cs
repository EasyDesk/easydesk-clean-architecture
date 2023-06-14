using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class DefaultIdentityPermissionsProvider : IIdentityPermissionsProvider
{
    private readonly IIdentityRolesProvider _identityRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public DefaultIdentityPermissionsProvider(IIdentityRolesProvider identityRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
    {
        _identityRolesProvider = identityRolesProvider;
        _rolesToPermissionsMapper = rolesToPermissionsMapper;
    }

    public async Task<IImmutableSet<Permission>> GetPermissionsForIdentity(Identity identity)
    {
        var roles = await _identityRolesProvider.GetRolesForIdentity(identity);
        return await _rolesToPermissionsMapper.MapRolesToPermissions(roles);
    }
}
