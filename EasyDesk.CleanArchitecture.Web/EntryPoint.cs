using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace EasyDesk.CleanArchitecture.Web
{
    public static class EntryPoint
    {
        public static IHostBuilder CreateHostBuilder<T>(string[] args)
            where T : class
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    var environment = env.EnvironmentName;
                    var mainFileName = "appSettings";
                    var sharedFileName = "sharedSettings";
                    var sharedFileOutsideProjectFolder = Path.Combine(env.ContentRootPath, "..", "..", "SharedConfig", sharedFileName);
                    var sharedFileInsideDocker = Path.Combine(env.ContentRootPath, "..", "SharedConfig", sharedFileName);

                    config.AddJsonFileWithEnvironment(sharedFileName, environment);
                    config.AddJsonFileWithEnvironment(sharedFileOutsideProjectFolder, environment);
                    config.AddJsonFileWithEnvironment(mainFileName, environment);

                    config.AddEnvironmentVariables();

                    config.AddEnvironmentVariables("EASYDESK_");

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
}
