using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public class AgentProvider : IAgentProvider
{
    private Option<Option<Agent>> _agent = None;

    public Option<Agent> Agent => _agent.OrElseThrow(() => new InvalidOperationException("Trying to access agent before initialization."));

    internal void InitializeAgent(Option<Agent> agent)
    {
        if (_agent.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize agent when it is already initialized.");
        }

        _agent = Some(agent);
    }
}
