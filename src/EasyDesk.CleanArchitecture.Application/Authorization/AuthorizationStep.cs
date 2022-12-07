using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class AuthorizationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IAuthorizer _authorizer;
    private readonly IContextProvider _contextProvider;

    public AuthorizationStep(IAuthorizer authorizer, IContextProvider contextProvider)
    {
        _authorizer = authorizer;
        _contextProvider = contextProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return _contextProvider.Context switch
        {
            AuthenticatedRequestContext(var userInfo) => await HandleAuthenticatedRequest(request, userInfo, next),
            AnonymousRequestContext => await HandleUnknownUserRequest(next),
            _ => await next()
        };
    }

    private async Task<Result<R>> HandleAuthenticatedRequest(
        T request, UserInfo userInfo, NextPipelineStep<R> next)
    {
        var isAuthorized = await _authorizer.IsAuthorized(request, userInfo);
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Result<R>> HandleUnknownUserRequest(NextPipelineStep<R> next) =>
        UnknownUserIsAllowed() ? await next() : new UnknownUserError();

    private bool UnknownUserIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownUserAttribute>() is not null;
}
