using EasyDesk.CleanArchitecture.Application.Authorization.Model;
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
    public static async Task<Result<T>> Require<T>(this IAuthorizationProvider provider, AsyncFunc<AuthorizationInfo, Option<T>> predicate)
    {
        return await provider
            .GetAuthorizationInfo()
            .ThenOrElseError(() => new UnknownAgentError())
            .ThenFlatMapAsync(info => predicate(info).ThenOrElseError(() => Errors.Forbidden()));
    }

    public static async Task<Result<T>> Require<T>(this IAuthorizationProvider provider, Func<AuthorizationInfo, Option<T>> predicate) =>
        await provider.Require(c => Task.FromResult(predicate(c)));

    public static async Task<Result<Nothing>> Require(this IAuthorizationProvider provider, AsyncFunc<AuthorizationInfo, bool> predicate) =>
        await provider.Require(async c => await predicate(c) ? Some(Nothing.Value) : None);

    public static async Task<Result<Nothing>> Require(this IAuthorizationProvider provider, Func<AuthorizationInfo, bool> predicate) =>
        await provider.Require(c => Task.FromResult(predicate(c)));
}
