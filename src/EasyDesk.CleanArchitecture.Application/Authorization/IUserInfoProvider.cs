using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IUserInfoProvider
{
    public Option<UserInfo> GetUserInfo();
}
