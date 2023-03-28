namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public static class ContextExtensions
{
    public static UserInfo RequireUserInfo(this IUserInfoProvider userInfo) =>
        userInfo.UserInfo
            .OrElseThrow(
                () => new InvalidOperationException("Request context: request is not authenticated."));
}
