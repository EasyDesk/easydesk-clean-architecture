﻿using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public sealed class StaticAuthorizationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IContextProvider _contextProvider;
    private readonly IEnumerable<IStaticAuthorizer<T>> _authorizers;
    private readonly IAuthorizationProvider _authorizationProvider;

    public StaticAuthorizationStep(
        IContextProvider contextProvider,
        IEnumerable<IStaticAuthorizer<T>> authorizers,
        IAuthorizationProvider authorizationProvider)
    {
        _contextProvider = contextProvider;
        _authorizers = authorizers;
        _authorizationProvider = authorizationProvider;
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
        return await _authorizationProvider.GetAuthorizationInfo().ThenMatchAsync(
            some: authInfo => HandleAuthenticatedRequest(request, authInfo, next),
            none: () => HandleUnknownIdentityRequest(next));
    }

    private async Task<Result<R>> HandleAuthenticatedRequest(T request, AuthorizationInfo authInfo, NextPipelineStep<R> next)
    {
        return _authorizers.All(x => x.IsAuthorized(request, authInfo))
            ? await next()
            : Errors.Forbidden();
    }

    private async Task<Result<R>> HandleUnknownIdentityRequest(NextPipelineStep<R> next) =>
        UnknownIdentityIsAllowed() ? await next() : new UnknownIdentityError();

    private bool UnknownIdentityIsAllowed() => typeof(T).GetCustomAttribute<AllowUnknownIdentityAttribute>() is not null;
}