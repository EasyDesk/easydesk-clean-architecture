﻿using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class AuthenticationModule : IAppModule
{
    public AuthenticationModule(IImmutableList<IAuthenticationScheme> schemes)
    {
        Schemes = schemes;
    }

    public IImmutableList<IAuthenticationScheme> Schemes { get; }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<IUserInfoProvider, HttpContextUserInfoProvider>();
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Schemes[0].Name;
            options.DefaultChallengeScheme = Schemes[0].Name;
        });
        Schemes.ForEach(scheme =>
        {
            scheme.AddUtilityServices(services);
            scheme.AddAuthenticationHandler(authBuilder);
        });
    }
}

public static class AuthenticationModuleExtensions
{
    public static AppBuilder AddAuthentication(this AppBuilder builder, Action<AuthenticationOptions> configure)
    {
        var authOptions = new AuthenticationOptions();
        configure(authOptions);
        if (authOptions.Schemes.Count == 0)
        {
            throw new Exception("No authentication scheme was specified");
        }
        return builder.AddModule(new AuthenticationModule(authOptions.Schemes));
    }

    public static bool HasAuthentication(this AppDescription app) => app.HasModule<AuthenticationModule>();
}

public class AuthenticationOptions
{
    public IImmutableList<IAuthenticationScheme> Schemes { get; private set; } = List<IAuthenticationScheme>();

    public AuthenticationOptions AddScheme(IAuthenticationScheme scheme)
    {
        Schemes = Schemes.Add(scheme);
        return this;
    }
}