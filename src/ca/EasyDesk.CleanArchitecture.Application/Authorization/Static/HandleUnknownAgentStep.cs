using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public class HandleUnknownAgentStep<T, R> : IPipelineStep<T, R>
{
    private readonly IContextProvider _contextProvider;

    public HandleUnknownAgentStep(IContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return _contextProvider.CurrentContext switch
        {
            ContextInfo.AnonymousRequest => await HandleUnknownAgentRequest(next),
            _ => await next(),
        };
    }

    private async Task<Result<R>> HandleUnknownAgentRequest(NextPipelineStep<R> next) =>
        UnknownAgentIsAllowed() ? await next() : new UnknownAgentError();

    private bool UnknownAgentIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownAgentAttribute>() is not null;
}
