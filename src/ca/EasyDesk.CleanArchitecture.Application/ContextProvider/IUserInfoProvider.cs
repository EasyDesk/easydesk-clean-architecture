namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IUserInfoProvider
{
    Option<UserInfo> User { get; }
}
