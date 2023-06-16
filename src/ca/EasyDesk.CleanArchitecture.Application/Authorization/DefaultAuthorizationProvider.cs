using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class DefaultAuthorizationProvider : IAuthorizationProvider
{
    private readonly IContextProvider _contextProvider;
    private readonly IAgentPermissionsProvider _identityPermissionsProvider;

    public DefaultAuthorizationProvider(IContextProvider contextProvider, IAgentPermissionsProvider identityPermissionsProvider)
    {
        _contextProvider = contextProvider;
        _identityPermissionsProvider = identityPermissionsProvider;
    }

    public async Task<Option<AuthorizationInfo>> GetAuthorizationInfo()
    {
        return await _contextProvider
            .GetAgent()
            .MapAsync(agent => _identityPermissionsProvider
                .GetPermissionsForAgent(agent)
                .Map(p => new AuthorizationInfo(agent, p)));
    }
}
