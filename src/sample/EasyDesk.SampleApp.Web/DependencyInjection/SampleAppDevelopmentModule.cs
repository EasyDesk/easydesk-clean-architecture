using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using System.CommandLine;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDevelopmentModule : AppModule
{
    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        services.AddScoped<DevelopmentSeeder>();
        services.AddScoped(p => Seed(p.GetRequiredService<DevelopmentSeeder>()));
    }

    private Command Seed(DevelopmentSeeder seeder)
    {
        var developmentCommand = new Command("seed-dev", description: "Seed the database with development data.");
        developmentCommand.SetHandler(seeder.Seed);
        return developmentCommand;
    }
}
