using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record RemoveAdmin : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class RemoveAdminHandler : IHandler<RemoveAdmin>
{
    private readonly IIdentityRolesManager _identityRolesManager;
    private readonly IAgentProvider _agentProvider;

    public RemoveAdminHandler(IIdentityRolesManager identityRolesManager, IAgentProvider agentProvider)
    {
        _identityRolesManager = identityRolesManager;
        _agentProvider = agentProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveAdmin request)
    {
        await _identityRolesManager.RevokeRoles(_agentProvider.RequireAgent().MainIdentity(), Roles.Admin);
        return Ok;
    }
}
