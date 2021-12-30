using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Startup;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Web
{
    public class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment) : base(configuration, environment)
        {
        }

        protected override Type ApplicationAssemblyMarker => typeof(CoupleGotMarried);

        protected override Type InfrastructureAssemblyMarker => typeof(SampleAppContext);

        protected override Type WebAssemblyMarker => typeof(Startup);

        protected override bool IsMultitenant => true;

        protected override bool UsesSwagger => true;

        protected override bool UsesPublisher => true;

        protected override bool UsesConsumer => true;

        protected override IDataAccessImplementation DataAccessImplementation =>
            new EfCoreDataAccess<SampleAppContext>(Configuration, applyMigrations: true);

        protected override IEventBusImplementation EventBusImplementation =>
            new AzureServiceBus(Configuration, prefix: Environment.EnvironmentName);

        protected override string ServiceName => "Sample App";

        protected override void ConfigureMvc(MvcOptions options)
        {
            options.Filters.Add<TenantTestFilter>();
        }

        protected override void SetupAuthentication(AuthenticationOptions options)
        {
            options.AddScheme(new JwtBearerScheme(options =>
            {
                options.UseJwtSettingsFromConfiguration(Configuration);
            }));
        }
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
