using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Scopes;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public record DefaultTestAgent(Option<Agent> Agent);

public class TestAuthenticationManager : IHttpRequestConfigurator
{
    private readonly ScopeManager<Option<Agent>> _agentScopeManager;

    public TestAuthenticationManager(DefaultTestAgent defaultAgent)
    {
        _agentScopeManager = new(defaultAgent.Agent);
    }

    public AgentScope AuthenticateAs(Agent agent)
    {
        return new(_agentScopeManager.OpenScope(Some(agent)));
    }

    public AgentScope Anonymize()
    {
        return new(_agentScopeManager.OpenScope(None));
    }

    public void ConfigureHttpRequest(HttpRequestBuilder request)
    {
        _agentScopeManager.Current.Match(
            some: request.AuthenticateAs,
            none: request.NoAuthentication);
    }
}

public class AgentScope : IDisposable
{
    private readonly ScopeManager<Option<Agent>>.Scope _scope;

    public AgentScope(ScopeManager<Option<Agent>>.Scope scope)
    {
        _scope = scope;
    }

    public Option<Agent> Agent => _scope.Value;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _scope.Dispose();
    }
}
