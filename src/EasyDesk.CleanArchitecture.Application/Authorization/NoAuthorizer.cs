using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class NoAuthorizer : IAuthorizer
{
    public Task<bool> IsAuthorized<T>(T request, UserInfo userInfo) where T : notnull => Task.FromResult(true);
}
