using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

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

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(Options);

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Options.DefaultScheme;
            options.DefaultChallengeScheme = Options.DefaultScheme;
        });

        Options.Schemes.ForEach(scheme =>
        {
            scheme.Value.AddUtilityServices(services, app);
            scheme.Value.AddAuthenticationHandler(scheme.Key, authBuilder);
        });

        services.AddAuthorizationBuilder()
            .AddFallbackPolicy("DEFAULT", new AuthorizationPolicyBuilder(Options.Schemes.Keys.ToArray())
                .RequireAssertion(_ => true)
                .Build());
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
    public string? DefaultScheme { get; private set; }

    public IFixedMap<string, IAuthenticationProvider> Schemes { get; private set; } = Map<string, IAuthenticationProvider>();

    public AuthenticationModuleOptions AddScheme(string name, IAuthenticationProvider provider)
    {
        if (Schemes.IsEmpty())
        {
            DefaultScheme = name;
        }
        Schemes = Schemes.Add(name, provider);
        return this;
    }
}
