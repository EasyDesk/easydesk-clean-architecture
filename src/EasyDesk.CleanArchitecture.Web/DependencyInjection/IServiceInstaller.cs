using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection;

public interface IServiceInstaller
{
    void InstallServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment);
}
