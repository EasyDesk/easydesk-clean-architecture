using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public class RoleBasedPermissionsProvider : IAgentPermissionsProvider
{
    private readonly IAgentRolesProvider _identityRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public RoleBasedPermissionsProvider(IAgentRolesProvider identityRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
    {
        _identityRolesProvider = identityRolesProvider;
        _rolesToPermissionsMapper = rolesToPermissionsMapper;
    }

    public async Task<IFixedSet<Permission>> GetPermissionsForAgent(Agent agent)
    {
        var roles = await _identityRolesProvider.GetRolesForAgent(agent);
        return await _rolesToPermissionsMapper.MapRolesToPermissions(roles);
    }
}
