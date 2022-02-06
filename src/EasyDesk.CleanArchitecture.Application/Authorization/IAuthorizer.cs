using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAuthorizer<T>
{
    Task<bool> IsAuthorized(T request, UserInfo userInfo);
}
