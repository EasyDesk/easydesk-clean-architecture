using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class DefaultAgentPermissionsProvider : IAgentPermissionsProvider
{
    private readonly IAgentRolesProvider _identityRolesProvider;
    private readonly IRolesToPermissionsMapper _rolesToPermissionsMapper;

    public DefaultAgentPermissionsProvider(IAgentRolesProvider identityRolesProvider, IRolesToPermissionsMapper rolesToPermissionsMapper)
    {
        _identityRolesProvider = identityRolesProvider;
        _rolesToPermissionsMapper = rolesToPermissionsMapper;
    }

    public async Task<IImmutableSet<Permission>> GetPermissionsForAgent(Agent agent)
    {
        var roles = await _identityRolesProvider.GetRolesForAgent(agent);
        return await _rolesToPermissionsMapper.MapRolesToPermissions(roles);
    }
}
