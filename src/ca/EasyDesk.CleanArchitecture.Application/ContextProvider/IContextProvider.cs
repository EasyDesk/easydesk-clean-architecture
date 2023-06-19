namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IContextProvider
{
    ContextInfo CurrentContext { get; }

    CancellationToken CancellationToken { get; }
}

public static class ContextProviderExtensions
{
    public static Option<Agent> GetAgent(this IContextProvider contextProvider) =>
        contextProvider.CurrentContext switch
        {
            ContextInfo.AuthenticatedRequest(var agent) => Some(agent),
            _ => None,
        };

    public static Agent RequireAgent(this IContextProvider contextProvider) =>
        contextProvider.GetAgent().OrElseThrow(
            () => new InvalidOperationException("Request context: request is not authenticated."));

    public static Identity RequireSingleIdentity(this IContextProvider contextProvider) =>
        contextProvider.RequireAgent().SingleIdentity();
}
