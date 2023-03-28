using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

public class AuthenticationModule : AppModule
{
    public AuthenticationModule(AuthenticationModuleOptions options)
    {
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
            scheme.Value.AddUtilityServices(services);
            scheme.Value.AddAuthenticationHandler(scheme.Key, authBuilder);
        });
    }
}

public static class AuthenticationModuleExtensions
{
    public static AppBuilder AddAuthentication(this AppBuilder builder, Action<AuthenticationModuleOptions> configure)
    {
        var authOptions = new AuthenticationModuleOptions();
        configure(authOptions);
        if (authOptions.Schemes.Count == 0)
        {
            throw new Exception("No authentication scheme was specified");
        }
        return builder.AddModule(new AuthenticationModule(authOptions));
    }

    public static bool HasAuthentication(this AppDescription app) => app.HasModule<AuthenticationModule>();
}

public class AuthenticationModuleOptions
{
    public string? DefaultScheme { get; private set; }

    public IImmutableDictionary<string, IAuthenticationProvider> Schemes { get; private set; } = Map<string, IAuthenticationProvider>();

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
