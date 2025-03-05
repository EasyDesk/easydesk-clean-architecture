using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using System.CommandLine;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDevelopmentModule : AppModule
{
    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<DevelopmentSeeder>()
            .InstancePerLifetimeScope();

        builder.Register(c => SeedCommand(c.Resolve<IComponentContext>()))
            .InstancePerLifetimeScope();
    }

    private Command SeedCommand(IComponentContext context)
    {
        var developmentCommand = new Command("seed-dev", description: "Seed the database with development data.");
        developmentCommand.SetHandler(() => context.Resolve<DevelopmentSeeder>().Seed());
        return developmentCommand;
    }
}
