﻿using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Web.Seeding;

public static class WebApplicationExtensions
{
    public static IDispatcher SetupSelfScopedDispatcher(this IServiceProvider services, AsyncAction<IServiceProvider> setupScope) =>
        new AutoScopingDispatcher(services, setupScope);

    public static IDispatcher SetupSelfScopedRequestDispatcher(this IServiceProvider services, string? userId = null, string? tenantId = null) =>
        SetupSelfScopedDispatcher(services, services =>
        {
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            if (userId != null)
            {
                httpContextAccessor.SetupAuthenticatedHttpContext(userId);
            }
            if (tenantId != null)
            {
                httpContextAccessor.SetupMultitenantHttpContext(tenantId);
            }
        });

    public static IDispatcher SetupSelfScopedDispatcher(this IServiceProvider services, Action<IServiceProvider> setupScope) =>
        new AutoScopingDispatcher(services, x =>
        {
            setupScope(x);
            return Task.CompletedTask;
        });

    public static async Task SetupDevelopment(this WebApplication app, AsyncAction<IServiceProvider, ILogger> setup)
    {
        if (app.Environment.IsDevelopment())
        {
            await setup(app.Services, app.Logger);
        }
    }
}