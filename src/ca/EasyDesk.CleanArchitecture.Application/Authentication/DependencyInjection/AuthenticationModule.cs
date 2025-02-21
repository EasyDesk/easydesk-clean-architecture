using Autofac;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;

public class AuthenticationModule : AppModule
{
    public AuthenticationModule(Action<AuthenticationModuleOptions> configure)
    {
        var options = new AuthenticationModuleOptions();
        configure(options);
        if (options.Schemes.Count == 0)
        {
            throw new Exception("No authentication scheme was specified");
        }
        Options = options;
    }

    public AuthenticationModuleOptions Options { get; }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStep(typeof(AuthenticationStep<,>))
            .After(typeof(UnitOfWorkStep<,>)));
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        services.AddSingleton(Options);

        services.AddTransient<IAuthenticationService, AuthenticationService>();

        builder.RegisterType<AgentProvider>()
            .AsSelf()
            .As<IAgentProvider>()
            .InstancePerLifetimeScope(); // TODO: request scoped

        Options.Schemes.ForEach(scheme =>
        {
            scheme.Value.AddUtilityServices(builder, services, app, scheme.Key);
            builder
                .Register(c => new AuthenticationScheme
                {
                    Scheme = scheme.Key,
                    Handler = scheme.Value.CreateHandler(c, scheme.Key),
                })
                .InstancePerDependency();
        });
    }
}

public static class AuthenticationModuleExtensions
{
    public static IAppBuilder AddAuthentication(this IAppBuilder builder, Action<AuthenticationModuleOptions> configure)
    {
        return builder.AddModule(new AuthenticationModule(configure));
    }

    public static bool HasAuthentication(this AppDescription app) => app.HasModule<AuthenticationModule>();
}

public sealed class AuthenticationModuleOptions
{
    public Option<string> DefaultScheme { get; private set; }

    public IFixedMap<string, IAuthenticationProvider> Schemes { get; private set; } = Map<string, IAuthenticationProvider>();

    public AuthenticationModuleOptions AddScheme(string name, IAuthenticationProvider provider)
    {
        DefaultScheme.IfAbsent(() => DefaultScheme = Some(name));
        Schemes = Schemes.Add(name, provider);
        return this;
    }
}
