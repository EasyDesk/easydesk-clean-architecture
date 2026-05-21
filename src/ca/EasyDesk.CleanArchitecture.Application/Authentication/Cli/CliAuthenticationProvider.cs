using Autofac;
using EasyDesk.CleanArchitecture.Application.CommandLine;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.Cli;

public delegate Task<Option<AuthenticationResult>> CliAuthenticationDelegate(CliContext cliContext);

public class CliAuthenticationProvider : IAuthenticationProvider
{
    private readonly Func<IComponentContext, CliAuthenticationDelegate> _authenticationDelegateFactory;

    public CliAuthenticationProvider(Func<IComponentContext, CliAuthenticationDelegate> authenticationDelegateFactory)
    {
        _authenticationDelegateFactory = authenticationDelegateFactory;
    }

    public void AddUtilityServices(ServiceRegistry registry, AppDescription app, string scheme)
    {
        registry.ConfigureContainer(builder =>
        {
            builder.Register(_authenticationDelegateFactory)
                .InstancePerDependency();
        });
    }

    public IAuthenticationHandler CreateHandler(IComponentContext context, string scheme) =>
        new CliAuthenticationHandler(
            context.Resolve<CliContextAccessor>(),
            context.Resolve<CliAuthenticationDelegate>());
}
