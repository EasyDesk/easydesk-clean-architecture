namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IUserInfoProvider
{
    public Option<UserInfo> GetUserInfo();
}
