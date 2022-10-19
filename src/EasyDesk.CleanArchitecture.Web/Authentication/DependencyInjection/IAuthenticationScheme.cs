using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

public interface IAuthenticationScheme
{
    void AddUtilityServices(IServiceCollection services);

    void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder);

    void ConfigureOpenApi(SwaggerGenOptions options);
}
