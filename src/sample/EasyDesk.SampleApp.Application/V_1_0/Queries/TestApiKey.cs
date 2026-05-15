using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.SampleApp.Application.V_1_0.Dto;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record TestApiKey : IQueryRequest<AgentDto>;

public class TestApiKeyHandler : SuccessHandler<TestApiKey, AgentDto>
{
    private readonly IAgentProvider _agentProvider;

    public TestApiKeyHandler(IAgentProvider agentProvider)
    {
        _agentProvider = agentProvider;
    }

    protected override Task<AgentDto> Process(TestApiKey request) =>
        Task.FromResult(AgentDto.MapFrom(_agentProvider.RequireAgent()));
}
