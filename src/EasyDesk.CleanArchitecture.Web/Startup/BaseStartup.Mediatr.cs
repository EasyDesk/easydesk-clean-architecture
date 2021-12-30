using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        private void AddMediatr(IServiceCollection services)
        {
            services
                .AddMediatR(ApplicationAssemblyMarker, InfrastructureAssemblyMarker)
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorWrapper<,>));
        }
    }
}
