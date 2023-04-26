namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IContextProvider
{
    ContextInfo CurrentContext { get; }

    CancellationToken CancellationToken { get; }
}

public static class ContextProviderExtensions
{
    public static Option<UserInfo> GetUserInfo(this IContextProvider contextProvider) =>
        contextProvider.CurrentContext switch
        {
            ContextInfo.AuthenticatedRequest(var info) => Some(info),
            _ => None,
        };

    public static UserInfo RequireUserInfo(this IContextProvider userInfo) =>
        userInfo.GetUserInfo().OrElseThrow(
            () => new InvalidOperationException("Request context: request is not authenticated."));
}
