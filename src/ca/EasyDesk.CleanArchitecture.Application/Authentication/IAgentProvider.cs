using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public interface IAgentProvider
{
    Option<Agent> Agent { get; }
}

public static class AgentProviderExtensions
{
    public static Agent RequireAgent(this IAgentProvider agentProvider) => agentProvider.Agent
        .OrElseThrow(() => new InvalidOperationException("Request context: request is not authenticated."));
}
