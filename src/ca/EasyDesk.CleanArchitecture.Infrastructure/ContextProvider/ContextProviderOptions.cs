using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class ContextProviderOptions
{
    public ClaimsPrincipalParser<Agent> AgentParser { get; set; } = ClaimsPrincipalParsers.ForDefaultAgent();

    public void SetAgentParser(Action<AgentParserBuilder> configure)
    {
        var builder = new AgentParserBuilder();
        configure(builder);
        AgentParser = builder.Build();
    }
}
