using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.SampleApp.Application.Commands;

public class RemoveRoles : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class RemoveRolesHandler : IHandler<RemoveRoles>
{
    private readonly IUserRolesManager _userRolesManager;
    private readonly IContextProvider _contextProvider;

    public RemoveRolesHandler(IUserRolesManager userRolesManager, IContextProvider contextProvider)
    {
        _userRolesManager = userRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveRoles request)
    {
        await _userRolesManager.RevokeAllRolesToUser(_contextProvider.RequireUserInfo().UserId);
        return Ok;
    }
}
