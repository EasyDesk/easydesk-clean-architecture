using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public interface IAuthenticationProvider
{
    void AddUtilityServices(ContainerBuilder builder, IServiceCollection services, AppDescription app, string scheme);

    IAuthenticationHandler CreateHandler(IComponentContext context, string scheme);
}
