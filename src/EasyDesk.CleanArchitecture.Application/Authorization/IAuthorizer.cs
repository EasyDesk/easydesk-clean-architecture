using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAuthorizer
{
    Task<bool> IsAuthorized<T>(T request, UserInfo userInfo);
}
