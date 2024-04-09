using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationProvider : IAuthorizationProvider
{
    private readonly IContextProvider _contextProvider;
    private readonly IAgentPermissionsProvider _agentPermissionsProvider;

    public DefaultAuthorizationProvider(IContextProvider contextProvider, IAgentPermissionsProvider agentPermissionsProvider)
    {
        _contextProvider = contextProvider;
        _agentPermissionsProvider = agentPermissionsProvider;
    }

    public async Task<Option<AuthorizationInfo>> GetAuthorizationInfo()
    {
        return await _contextProvider
            .GetAgent()
            .MapAsync(agent => _agentPermissionsProvider
                .GetPermissionsForAgent(agent)
                .Map(p => new AuthorizationInfo(agent, p)));
    }
}
