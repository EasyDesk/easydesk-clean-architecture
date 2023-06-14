using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public sealed class StaticAuthorizationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IContextProvider _contextProvider;
    private readonly IStaticAuthorizer _authorizer;
    private readonly IAuthorizationInfoProvider _authorizationInfoProvider;

    public StaticAuthorizationStep(IContextProvider contextProvider, IStaticAuthorizer authorizer, IAuthorizationInfoProvider authorizationInfoProvider)
    {
        _contextProvider = contextProvider;
        _authorizer = authorizer;
        _authorizationInfoProvider = authorizationInfoProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return _contextProvider.CurrentContext switch
        {
            ContextInfo.Request => await HandleRequest(request, next),
            _ => await next()
        };
    }

    private async Task<Result<R>> HandleRequest(T request, NextPipelineStep<R> next)
    {
        return await _authorizationInfoProvider.GetAuthorizationInfo().ThenMatchAsync(
            some: authInfo => HandleAuthenticatedRequest(request, authInfo, next),
            none: () => HandleUnknownIdentityRequest(next));
    }

    private async Task<Result<R>> HandleAuthenticatedRequest(T request, AuthorizationInfo authInfo, NextPipelineStep<R> next)
    {
        var isAuthorized = await _authorizer.IsAuthorized(request, authInfo);
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Result<R>> HandleUnknownIdentityRequest(NextPipelineStep<R> next) =>
        UnknownIdentityIsAllowed() ? await next() : new UnknownIdentityError();

    private bool UnknownIdentityIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownIdentityAttribute>() is not null;
}
