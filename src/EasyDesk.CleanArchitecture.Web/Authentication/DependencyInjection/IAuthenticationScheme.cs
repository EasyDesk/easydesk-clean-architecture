using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

public interface IAuthenticationScheme
{
    string Name { get; }

    void AddUtilityServices(IServiceCollection services);

    void AddAuthenticationHandler(AuthenticationBuilder authenticationBuilder);

    void ConfigureSwagger(SwaggerGenOptions options);
}
