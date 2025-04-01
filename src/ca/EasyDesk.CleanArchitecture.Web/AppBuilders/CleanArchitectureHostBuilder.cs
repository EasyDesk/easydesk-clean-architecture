using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Web.AppBuilders;

public sealed class CleanArchitectureHostBuilder : CleanArchitectureAppBuilder<HostApplicationBuilder, IHost>
{
    internal CleanArchitectureHostBuilder(string name, string[] commandArgs, string[] configurationArgs)
        : base(name, commandArgs, Host.CreateApplicationBuilder(configurationArgs))
    {
    }

    protected override IHost BuildHost(HostApplicationBuilder builder) => builder.Build();
}
