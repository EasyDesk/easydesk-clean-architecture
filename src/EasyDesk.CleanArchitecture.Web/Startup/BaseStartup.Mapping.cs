using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        private void AddMappings(IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                config.AddProfile(new DefaultMappingProfile(
                    typeof(SupportedVersionDto),
                    ApplicationAssemblyMarker,
                    WebAssemblyMarker,
                    InfrastructureAssemblyMarker));
            });
        }
    }
}
