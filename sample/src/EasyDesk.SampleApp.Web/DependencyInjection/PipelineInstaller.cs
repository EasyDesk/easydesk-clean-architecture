using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Web.DependencyInjection
{
    public class PipelineInstaller : PipelineInstallerBase
    {
        protected override Type ApplicationAssemblyMarker => typeof(CoupleGotMarried);

        protected override Type InfrastructureAssemblyMarker => typeof(SampleAppContext);

        protected override Type WebAssemblyMarker => typeof(Startup);

        protected override bool UsesPublisher => true;

        protected override bool UsesConsumer => true;

        protected override void ConfigureMvc(MvcOptions options, IConfiguration config, IWebHostEnvironment environment)
        {
            options.Filters.Add<TenantTestFilter>();
            base.ConfigureMvc(options, config, environment);
        }

        protected override IDataAccessImplementation GetDataAccessImplementation(IConfiguration configuration, IWebHostEnvironment environment) =>
            new EfCoreDataAccess<SampleAppContext>(configuration);

        protected override IEventBusImplementation GetEventBusImplementation(IConfiguration configuration, IWebHostEnvironment environment) =>
            new AzureServiceBusImplementation(configuration, environment);
    }

    public class TenantTestFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User.AddIdentity(new ClaimsIdentity(new Claim[] { new("tenantId", "test") }));
            await next();
        }
    }
}
