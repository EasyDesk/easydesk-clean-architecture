using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record RemoveAdmin : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class RemoveAdminHandler : IHandler<RemoveAdmin>
{
    private readonly IUserRolesManager _userRolesManager;
    private readonly IContextProvider _contextProvider;

    public RemoveAdminHandler(IUserRolesManager userRolesManager, IContextProvider contextProvider)
    {
        _userRolesManager = userRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(RemoveAdmin request)
    {
        await _userRolesManager.RevokeRolesToUser(_contextProvider.RequireUserInfo().UserId, Roles.Admin);
        return Ok;
    }
}
