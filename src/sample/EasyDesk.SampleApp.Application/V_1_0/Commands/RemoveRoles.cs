using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public class RemoveRoles : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class RemoveRolesHandler : IHandler<RemoveRoles>
{
    private readonly IIdentityRolesManager _identityRolesManager;
    private readonly IContextProvider _contextProvider;

    public RemoveRolesHandler(IIdentityRolesManager identityRolesManager, IContextProvider contextProvider)
    {
        _identityRolesManager = identityRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveRoles request)
    {
        await _identityRolesManager.RevokeAllRolesToIdentity(_contextProvider.RequireAgent().SingleIdentity().Id);
        return Ok;
    }
}
