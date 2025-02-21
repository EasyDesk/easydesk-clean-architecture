using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using System.Diagnostics;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public class AuthenticationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly AgentProvider _agentProvider;

    public AuthenticationStep(IAuthenticationService authenticationService, AgentProvider agentProvider)
    {
        _authenticationService = authenticationService;
        _agentProvider = agentProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await GetAgent(request)
            .ThenIfSuccess(_agentProvider.InitializeAgent)
            .ThenFlatMapAsync(_ => next());
    }

    private async Task<Result<Option<Agent>>> GetAgent(T request)
    {
        if (request is not IRequest)
        {
            return Success<Option<Agent>>(None);
        }

        var maybeResult = await _authenticationService.Authenticate();

        if (maybeResult.IsAbsent(out var result))
        {
            return Success<Option<Agent>>(None);
        }

        return result.Result switch
        {
            AuthenticationResult.Authenticated authenticated => Some(authenticated.Agent),
            AuthenticationResult.Failed failed => Errors.AuthenticationFailed(result.Scheme, failed.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    public bool IsForEachHandler => false;
}
