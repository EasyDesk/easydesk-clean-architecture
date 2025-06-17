using Autofac;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;

public class AuthenticationModule : AppModule
{
    public AuthenticationModule(AuthenticationModuleOptions options)
    {
        if (options.Schemes.Count == 0)
        {
            throw new ArgumentException("No authentication scheme was specified");
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

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        Options.Schemes.ForEach(scheme =>
        {
            scheme.Provider.AddUtilityServices(registry, app, scheme.Name);
        });
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(Options).SingleInstance();

        builder.RegisterType<AuthenticationService>()
            .As<IAuthenticationService>()
            .InstancePerDependency();

        builder.RegisterType<AgentProvider>()
            .AsSelf()
            .As<IAgentProvider>()
            .InstancePerUseCase();

        Options.Schemes.ForEach(scheme =>
        {
            builder
                .Register(c => new AuthenticationScheme
                {
                    Scheme = scheme.Name,
                    Handler = scheme.Provider.CreateHandler(c, scheme.Name),
                })
                .InstancePerDependency();
        });
    }
}

public static class AuthenticationModuleExtensions
{
    public static IAppBuilder AddAuthentication(this IAppBuilder builder, Action<AuthenticationModuleOptions> configure)
    {
        return builder.AddModule(new AuthenticationModule(new AuthenticationModuleOptions().Also(configure)));
    }

    public static bool HasAuthentication(this AppDescription app) => app.HasModule<AuthenticationModule>();
}

public sealed class AuthenticationModuleOptions
{
    public Option<string> DefaultScheme { get; private set; }

    public IFixedList<(string Name, IAuthenticationProvider Provider)> Schemes { get; private set; } = [];

    public AuthenticationModuleOptions AddScheme(string name, IAuthenticationProvider provider)
    {
        if (Schemes.Any(s => s.Name == name))
        {
            throw new InvalidOperationException($"An authentication scheme named {name} already exists.");
        }

        DefaultScheme.IfAbsent(() => DefaultScheme = Some(name));
        Schemes = Schemes.Add((name, provider));
        return this;
    }

    public Option<IAuthenticationProvider> GetSchemeProvider(string schemeName) => Schemes
        .SingleOption(x => x.Name == schemeName)
        .Map(x => x.Provider);
}
