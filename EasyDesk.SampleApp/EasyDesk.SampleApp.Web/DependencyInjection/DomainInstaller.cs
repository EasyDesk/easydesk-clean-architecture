using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.SampleApp.Web.DependencyInjection
{
    public class DomainInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddScoped<IPersonRepository>(_ => throw new NotImplementedException());
        }
    }
}
