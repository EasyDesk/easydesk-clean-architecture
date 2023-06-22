using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record AddAdmin : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class AddAdminHandler : IHandler<AddAdmin>
{
    private readonly IIdentityRolesManager _identityRolesManager;
    private readonly IContextProvider _contextProvider;

    public AddAdminHandler(IIdentityRolesManager identityRolesManager, IContextProvider contextProvider)
    {
        _identityRolesManager = identityRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(AddAdmin request)
    {
        await _identityRolesManager.GrantRoles(_contextProvider.RequireAgent().MainIdentity(), Roles.Admin);
        return Ok;
    }
}
