namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class NoAuthorizer : IAuthorizer
{
    public Task<bool> IsAuthorized<T>(T request, UserInfo userInfo) => Task.FromResult(true);
}
