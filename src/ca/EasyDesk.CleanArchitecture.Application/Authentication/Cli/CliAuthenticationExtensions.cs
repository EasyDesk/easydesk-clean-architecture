using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authentication.Cli;

public static class CliAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddCli(
        this AuthenticationModuleOptions options,
        string schemeName,
        Func<IComponentContext, CliAuthenticationDelegate> authenticationDelegateFactory)
    {
        return options.AddScheme(schemeName, new CliAuthenticationProvider(authenticationDelegateFactory));
    }

    public static AuthenticationModuleOptions AddCli(
        this AuthenticationModuleOptions options,
        Func<IComponentContext, CliAuthenticationDelegate> authenticationDelegateFactory)
    {
        return options.AddCli(CliAuthenticationDefaults.Scheme, authenticationDelegateFactory);
    }
}
