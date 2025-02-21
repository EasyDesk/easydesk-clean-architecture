using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationProvider : IAuthorizationProvider
{
    private readonly IAgentProvider _agentProvider;
    private readonly IAgentPermissionsProvider _agentPermissionsProvider;

    public DefaultAuthorizationProvider(IAgentProvider agentProvider, IAgentPermissionsProvider agentPermissionsProvider)
    {
        _agentProvider = agentProvider;
        _agentPermissionsProvider = agentPermissionsProvider;
    }

    public async Task<Option<AuthorizationInfo>> GetAuthorizationInfo()
    {
        return await _agentProvider.Agent
            .MapAsync(agent => _agentPermissionsProvider
                .GetPermissionsForAgent(agent)
                .Map(p => new AuthorizationInfo(agent, p)));
    }
}
