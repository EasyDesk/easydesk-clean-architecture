using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools.Collections;
using MediatR;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Response<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    private readonly IPermissionsProvider _permissionsProvider;
    private readonly IUserInfoProvider _userInfoProvider;

    public AuthorizationBehavior(IPermissionsProvider permissionsProvider, IUserInfoProvider userInfoProvider)
    {
        _permissionsProvider = permissionsProvider;
        _userInfoProvider = userInfoProvider;
    }

    public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Response<TResponse>> next)
    {
        return await _userInfoProvider.GetUserInfo().Match(
            some: userInfo => HandleAuthenticatedRequest(userInfo, next),
            none: () => HandleUnknownUserRequest(next));
    }

    private async Task<Response<TResponse>> HandleAuthenticatedRequest(UserInfo userInfo, RequestHandlerDelegate<Response<TResponse>> next)
    {
        var requirementAttributes = typeof(TRequest).GetCustomAttributes<RequireAnyOfAttribute>();
        if (requirementAttributes.IsEmpty())
        {
            return await next();
        }

        var userPermissions = await _permissionsProvider.GetPermissionsForUser(userInfo);
        var isAuthorized = requirementAttributes.All(a => IsAuthorized(a, userPermissions));
        return isAuthorized ? await next() : Errors.Forbidden();
    }

    private async Task<Response<TResponse>> HandleUnknownUserRequest(RequestHandlerDelegate<Response<TResponse>> next) =>
        UnknownUserIsAllowed() ? await next() : Errors.UnknownUser();

    private bool UnknownUserIsAllowed() => typeof(TRequest).GetCustomAttribute<AllowUnknownUserAttribute>() is not null;

    private bool IsAuthorized(RequireAnyOfAttribute attribute, IImmutableSet<Permission> userPermissions) =>
        userPermissions.Overlaps(attribute.Permissions);
}

public class AuthorizationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IPermissionsProvider _permissionsProvider;
    private readonly IUserInfoProvider _userInfo;

    public AuthorizationBehaviorWrapper(IPermissionsProvider permissionsProvider, IUserInfoProvider userInfo)
    {
        _permissionsProvider = permissionsProvider;
        _userInfo = userInfo;
    }

    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(AuthorizationBehavior<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType, _permissionsProvider, _userInfo) as IPipelineBehavior<TRequest, TResponse>;
    }
}
