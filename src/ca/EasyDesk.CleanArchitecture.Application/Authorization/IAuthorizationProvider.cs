﻿using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAuthorizationProvider
{
    Task<Option<AuthorizationInfo>> GetAuthorizationInfo();
}

public static class AuthorizationProviderExtensions
{
    public static async Task<Result<Nothing>> Require(this IAuthorizationProvider provider, AsyncFunc<AuthorizationInfo, bool> predicate)
    {
        return await provider
            .GetAuthorizationInfo()
            .ThenOrElseError(() => new UnknownAgentError())
            .ThenFilterAsync(predicate, otherwise: _ => Errors.Forbidden());
    }

    public static async Task<Result<Nothing>> Require(this IAuthorizationProvider provider, Func<AuthorizationInfo, bool> predicate) =>
        await provider.Require(c => Task.FromResult(predicate(c)));
}
