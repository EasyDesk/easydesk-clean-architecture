namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IContextProvider
{
    ContextInfo CurrentContext { get; }

    CancellationToken CancellationToken { get; }
}

public static class ContextProviderExtensions
{
    public static Option<Identity> GetIdentity(this IContextProvider contextProvider) =>
        contextProvider.CurrentContext switch
        {
            ContextInfo.AuthenticatedRequest(var info) => Some(info),
            _ => None,
        };

    public static Identity RequireIdentity(this IContextProvider contextProvider) =>
        contextProvider.GetIdentity().OrElseThrow(
            () => new InvalidOperationException("Request context: request is not authenticated."));
}
