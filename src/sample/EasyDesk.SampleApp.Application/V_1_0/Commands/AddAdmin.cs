﻿using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
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
    private readonly IUserRolesManager _userRolesManager;
    private readonly IContextProvider _contextProvider;

    public AddAdminHandler(IUserRolesManager userRolesManager, IContextProvider contextProvider)
    {
        _userRolesManager = userRolesManager;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Handle(AddAdmin request)
    {
        await _userRolesManager.GrantRolesToUser(_contextProvider.RequireUserInfo().UserId, Roles.Admin);
        return Ok;
    }
}