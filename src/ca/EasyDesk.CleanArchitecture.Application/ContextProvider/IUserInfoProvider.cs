namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IUserInfoProvider
{
    Option<UserInfo> UserInfo { get; }
}
