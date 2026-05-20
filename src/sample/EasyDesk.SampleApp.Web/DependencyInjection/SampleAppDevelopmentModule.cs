using Autofac;
using EasyDesk.CleanArchitecture.Application.CommandLine;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDevelopmentModule : AppModule
{
    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<DevelopmentSeeder>()
            .InstancePerLifetimeScope();

        builder.RegisterCliCommand("seed-dev", SeedCommand);
    }

    private void SeedCommand(CliCommandBuilder builder, IComponentContext componentContext)
    {
        builder
            .AddDescription("Seed the database with development data.")
            .HandleWith(_ => componentContext.Resolve<DevelopmentSeeder>().Seed());
    }
}
