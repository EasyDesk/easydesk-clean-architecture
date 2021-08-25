using EasyDesk.CleanArchitecture.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.SampleApp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return EntryPoint.CreateHostBuilder<Startup>(args, "SAMPLEAPP_");
        }
    }
}
