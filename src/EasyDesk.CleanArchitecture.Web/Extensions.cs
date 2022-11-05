using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Web;

public static class Extensions
{
    /// <summary>
    /// Override this method to setup additional modules to the default list:
    /// <list type="bullet">
    ///     <item><see cref="ControllersModule"/></item>
    ///     <item><see cref="JsonModule"/></item>
    ///     <item><see cref="DomainLayerModule"/></item>
    ///     <item><see cref="ContextProviderModule"/></item>
    ///     <item><see cref="TimeManagementModule"/></item>
    ///     <item><see cref="DispatchingModule"/></item>
    ///     <item><see cref="ValidationModule"/></item>
    /// </list>
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <param name="configure">Additional configuration for the <see cref="AppBuilder"/>.</param>
    /// <returns>The <see cref="AppDescription"/> created after configuration.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static AppDescription ConfigureForCleanArchitecture(
        this WebApplicationBuilder builder,
        Action<AppBuilder> configure = null)
    {
        var callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name;
        var match = Regex.Match(callingAssemblyName, @"^(.+)\.Web$");
        var assemblyPrefix = match.Success ? match.Groups[1].Value : callingAssemblyName;

        var appBuilder = new AppBuilder(assemblyPrefix)
            .AddControllers(builder.Environment)
            .AddJsonSerialization()
            .AddDomainLayer()
            .AddContextProvider()
            .AddTimeManagement()
            .AddDispatching()
            .AddValidation();

        configure?.Invoke(appBuilder);

        var appDescription = appBuilder.Build();

        appDescription.ConfigureServices(builder.Services);

        return appDescription;
    }
}
