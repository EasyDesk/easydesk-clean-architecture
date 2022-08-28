using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class AuthorizationStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICqrsRequest<TResult>
{
    private readonly IAuthorizer<TRequest> _authorizer;
    private readonly IUserInfoProvider _userInfoProvider;

    public AuthorizationStep(IAuthorizer<TRequest> authorizer, IUserInfoProvider userInfoProvider)
    {
        _authorizer = authorizer;
        _userInfoProvider = userInfoProvider;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
    {
        return await _userInfoProvider.GetUserInfo().Match(
            some: userInfo => HandleAuthenticatedRequest(request, userInfo, next),
            none: () => HandleUnknownUserRequest(next));
    }

    private async Task<Result<TResult>> HandleAuthenticatedRequest(TRequest request, UserInfo userInfo, NextPipelineStep<TResult> next)
    {
        var isAuthorized = await _authorizer.IsAuthorized(request, userInfo);
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Result<TResult>> HandleUnknownUserRequest(NextPipelineStep<TResult> next) =>
        UnknownUserIsAllowed() ? await next() : Errors.UnknownUser();

    private bool UnknownUserIsAllowed() => typeof(TRequest).GetCustomAttribute<AllowUnknownUserAttribute>() is not null;
}
