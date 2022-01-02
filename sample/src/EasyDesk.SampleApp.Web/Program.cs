using EasyDesk.CleanArchitecture.Web;
using EasyDesk.SampleApp.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

EntryPoint.CreateHostBuilder<Startup>(args, "SAMPLEAPP_")
    .Build()
    .Run();
