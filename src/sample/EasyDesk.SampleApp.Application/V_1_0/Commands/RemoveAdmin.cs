using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
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
    private readonly IContextProvider _contextProvider;

    public RemoveAdminHandler(IIdentityRolesManager identityRolesManager, IContextProvider contextProvider)
    {
        _identityRolesManager = identityRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveAdmin request)
    {
        await _identityRolesManager.RevokeRoles(_contextProvider.RequireAgent().MainIdentity(), Roles.Admin);
        return Ok;
    }
}
