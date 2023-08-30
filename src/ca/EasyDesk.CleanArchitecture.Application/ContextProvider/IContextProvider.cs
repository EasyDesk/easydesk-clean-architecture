using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IContextProvider
{
    ContextInfo CurrentContext { get; }

    Option<string> TenantId { get; }

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
}
