using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public class RemoveRoles : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class RemoveRolesHandler : IHandler<RemoveRoles>
{
    private readonly IIdentityRolesManager _identityRolesManager;
    private readonly IAgentProvider _agentProvider;

    public RemoveRolesHandler(IIdentityRolesManager identityRolesManager, IAgentProvider agentProvider)
    {
        _identityRolesManager = identityRolesManager;
        _agentProvider = agentProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveRoles request)
    {
        await _identityRolesManager.RevokeAllRoles(_agentProvider.RequireAgent().MainIdentity());
        return Ok;
    }
}
