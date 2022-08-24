namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class NoAuthorizer<T> : IAuthorizer<T>
{
    public Task<bool> IsAuthorized(T request, UserInfo userInfo) => Task.FromResult(true);
}
