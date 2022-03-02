using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools.Results;
using MediatR;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    private readonly IAuthorizer<TRequest> _authorizer;
    private readonly IUserInfoProvider _userInfoProvider;

    public AuthorizationBehavior(IAuthorizer<TRequest> authorizer, IUserInfoProvider userInfoProvider)
    {
        _authorizer = authorizer;
        _userInfoProvider = userInfoProvider;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Result<TResponse>> next)
    {
        return await _userInfoProvider.GetUserInfo().Match(
            some: userInfo => HandleAuthenticatedRequest(request, userInfo, next),
            none: () => HandleUnknownUserRequest(next));
    }

    private async Task<Result<TResponse>> HandleAuthenticatedRequest(TRequest request, UserInfo userInfo, RequestHandlerDelegate<Result<TResponse>> next)
    {
        var isAuthorized = await _authorizer.IsAuthorized(request, userInfo);
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Result<TResponse>> HandleUnknownUserRequest(RequestHandlerDelegate<Result<TResponse>> next) =>
        UnknownUserIsAllowed() ? await next() : Errors.UnknownUser();

    private bool UnknownUserIsAllowed() => typeof(TRequest).GetCustomAttribute<AllowUnknownUserAttribute>() is not null;
}

public class AuthorizationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuthorizer<TRequest> _authorizer;
    private readonly IUserInfoProvider _userInfoProvider;

    public AuthorizationBehaviorWrapper(IAuthorizer<TRequest> authorizer, IUserInfoProvider userInfoProvider)
    {
        _authorizer = authorizer;
        _userInfoProvider = userInfoProvider;
    }

    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(AuthorizationBehavior<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType, _authorizer, _userInfoProvider) as IPipelineBehavior<TRequest, TResponse>;
    }
}
