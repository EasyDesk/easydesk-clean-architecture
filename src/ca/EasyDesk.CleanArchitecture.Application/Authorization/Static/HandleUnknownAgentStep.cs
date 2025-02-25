using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public class HandleUnknownAgentStep<T, R> : IPipelineStep<T, R>
{
    private readonly IAgentProvider _agentProvider;

    public HandleUnknownAgentStep(IAgentProvider agentProvider)
    {
        _agentProvider = agentProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return request switch
        {
            IRequest when _agentProvider.Agent.IsAbsent => await HandleUnknownAgentRequest(next),
            _ => await next(),
        };
    }

    private async Task<Result<R>> HandleUnknownAgentRequest(NextPipelineStep<R> next) =>
        UnknownAgentIsAllowed() ? await next() : new UnknownAgentError();

    private bool UnknownAgentIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownAgentAttribute>() is not null;
}
