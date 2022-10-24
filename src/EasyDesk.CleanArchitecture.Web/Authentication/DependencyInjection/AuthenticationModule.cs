using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

public class AuthenticationModule : AppModule
{
    private readonly string _defaultScheme;

    public AuthenticationModule(string defaultScheme, IImmutableDictionary<string, IAuthenticationScheme> schemes)
    {
        _defaultScheme = defaultScheme;
        Schemes = schemes;
    }

    public IImmutableDictionary<string, IAuthenticationScheme> Schemes { get; }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<IUserInfoProvider, HttpContextUserInfoProvider>();
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = _defaultScheme;
            options.DefaultChallengeScheme = _defaultScheme;
        });
        Schemes.ForEach(scheme =>
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
        return builder.AddModule(new AuthenticationModule(authOptions.DefaultScheme, authOptions.Schemes));
    }

    public static bool HasAuthentication(this AppDescription app) => app.HasModule<AuthenticationModule>();
}

public class AuthenticationModuleOptions
{
    public string DefaultScheme { get; private set; }

    public IImmutableDictionary<string, IAuthenticationScheme> Schemes { get; private set; } = Map<string, IAuthenticationScheme>();

    public AuthenticationModuleOptions AddScheme(string name, IAuthenticationScheme scheme)
    {
        if (Schemes.IsEmpty())
        {
            DefaultScheme = name;
        }
        Schemes = Schemes.Add(name, scheme);
        return this;
    }
}
