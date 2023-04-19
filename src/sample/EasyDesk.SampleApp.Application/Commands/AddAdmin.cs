using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.Commands;

public record AddAdmin() : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.ExistingTenantOrPublic();
}

public class AddAdminHandler : IHandler<AddAdmin>
{
    private readonly IUserRolesManager _userRolesManager;
    private readonly IUserInfoProvider _userInfoProvider;

    public AddAdminHandler(IUserRolesManager userRolesManager, IUserInfoProvider userInfoProvider)
    {
        _userRolesManager = userRolesManager;
        _userInfoProvider = userInfoProvider;
    }

    public async Task<Result<Nothing>> Handle(AddAdmin request)
    {
        await _userRolesManager.GrantRolesToUser(_userInfoProvider.User.Value.UserId, Roles.Admin);
        return Ok;
    }
}
