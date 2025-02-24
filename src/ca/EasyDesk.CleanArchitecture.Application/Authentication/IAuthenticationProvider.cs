using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public interface IAuthenticationProvider
{
    void AddUtilityServices(ServiceRegistry registry, AppDescription app, string scheme);

    IAuthenticationHandler CreateHandler(IComponentContext context, string scheme);
}
