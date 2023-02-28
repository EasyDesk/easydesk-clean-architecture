using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.SampleApp.Application.Commands;

public record AddAdmin() : ICommandRequest<Nothing>;

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
        await _userRolesManager.GrantRolesToUser(_userInfoProvider.UserInfo.Value, Roles.Admin);
        return Ok;
    }
}
