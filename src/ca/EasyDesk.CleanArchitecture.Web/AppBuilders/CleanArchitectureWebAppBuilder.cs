using Microsoft.AspNetCore.Builder;

namespace EasyDesk.CleanArchitecture.Web.AppBuilders;

public sealed class CleanArchitectureWebAppBuilder : CleanArchitectureAppBuilder<WebApplicationBuilder, WebApplication>
{
    internal CleanArchitectureWebAppBuilder(string name, string[] commandArgs, string[] configurationArgs)
        : base(name, commandArgs, WebApplication.CreateBuilder(configurationArgs))
    {
    }

    protected override WebApplication BuildHost(WebApplicationBuilder builder) => builder.Build();
}
