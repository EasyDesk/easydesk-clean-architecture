using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Web;

public static class EntryPoint
{
    public static IHostBuilder CreateHostBuilder<T>(string[] args, string envPrefix)
        where T : class
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;

                var environmentName = env.EnvironmentName;
                var mainFileName = "appsettings";

                config.AddJsonFileWithEnvironment(mainFileName, environmentName);

                config.AddEnvironmentVariables();

                config.AddEnvironmentVariables(envPrefix);

                if (env.IsDevelopment())
                {
                    config.AddUserSecrets<T>(optional: true);
                }

                config.AddCommandLine(args);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<T>();
            });
    }

    private static void AddJsonFileWithEnvironment(this IConfigurationBuilder builder, string baseName, string environment)
    {
        var extension = "json";
        builder.AddJsonFile($"{baseName}.{extension}", optional: true, reloadOnChange: true);
        builder.AddJsonFile($"{baseName}.{environment}.{extension}", optional: true, reloadOnChange: true);
    }
}
