namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public static class ContextExtensions
{
    public static Option<UserInfo> GetUserInfo(this Context context) =>
        context switch
        {
            AuthenticatedRequestContext(UserInfo info) => Some(info),
            _ => None
        };

    public static UserInfo RequireUserInfo(this Context context) =>
        context switch
        {
            AuthenticatedRequestContext(UserInfo info) => info,
            _ => throw new InvalidOperationException("Request context: request is not authenticated.")
        };
}
