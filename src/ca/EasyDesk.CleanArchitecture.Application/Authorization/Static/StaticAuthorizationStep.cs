using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public sealed class StaticAuthorizationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IEnumerable<IStaticAuthorizer<T>> _authorizers;
    private readonly IAuthorizationProvider _authorizationProvider;

    public StaticAuthorizationStep(
        IEnumerable<IStaticAuthorizer<T>> authorizers,
        IAuthorizationProvider authorizationProvider)
    {
        _authorizers = authorizers;
        _authorizationProvider = authorizationProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _authorizationProvider.GetAuthorizationInfo().ThenMatchAsync(
            some: authInfo => HandleAuthenticatedRequest(request, authInfo, next),
            none: () => next());
    }

    private async Task<Result<R>> HandleAuthenticatedRequest(T request, AuthorizationInfo authInfo, NextPipelineStep<R> next)
    {
        return _authorizers.All(x => x.IsAuthorized(request, authInfo))
            ? await next()
            : Errors.Forbidden();
    }
}
