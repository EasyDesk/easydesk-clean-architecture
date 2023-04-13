namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public static class ContextExtensions
{
    public static UserInfo RequireUserInfo(this IUserInfoProvider userInfo) =>
        userInfo.User.OrElseThrow(
            () => new InvalidOperationException("Request context: request is not authenticated."));
}
