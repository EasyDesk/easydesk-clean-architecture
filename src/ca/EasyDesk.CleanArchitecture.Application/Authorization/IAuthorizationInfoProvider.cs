using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAuthorizationInfoProvider
{
    Task<Option<AuthorizationInfo>> GetAuthorizationInfo();
}

public static class AuthorizationInfoProviderExtensions
{
    public static async Task<Result<Nothing>> Require(this IAuthorizationInfoProvider provider, AsyncFunc<AuthorizationInfo, bool> predicate)
    {
        return await provider
            .GetAuthorizationInfo()
            .ThenFilterAsync(predicate)
            .ThenOrElseError(() => Errors.Forbidden());
    }
}
