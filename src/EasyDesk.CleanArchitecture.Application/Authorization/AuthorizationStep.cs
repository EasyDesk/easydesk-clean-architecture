using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class AuthorizationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IAuthorizer _authorizer;
    private readonly IUserInfoProvider _userInfoProvider;

    public AuthorizationStep(IAuthorizer authorizer, IUserInfoProvider userInfoProvider)
    {
        _authorizer = authorizer;
        _userInfoProvider = userInfoProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _userInfoProvider.GetUserInfo().Match(
            some: userInfo => HandleAuthenticatedRequest(request, userInfo, next),
            none: () => HandleUnknownUserRequest(next));
    }

    private async Task<Result<R>> HandleAuthenticatedRequest(
        T request, UserInfo userInfo, NextPipelineStep<R> next)
    {
        var isAuthorized = await _authorizer.IsAuthorized(request, userInfo);
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Result<R>> HandleUnknownUserRequest(NextPipelineStep<R> next) =>
        UnknownUserIsAllowed() ? await next() : Errors.UnknownUser();

    private bool UnknownUserIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownUserAttribute>() is not null;
}
