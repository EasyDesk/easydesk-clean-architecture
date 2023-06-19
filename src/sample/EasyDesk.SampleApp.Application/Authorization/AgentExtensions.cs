using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.SampleApp.Application.Authorization;

public static class AgentExtensions
{
    public static Identity MainIdentity(this Agent agent) =>
        agent.RequireIdentity(Agent.DefaultIdentityName);
}
