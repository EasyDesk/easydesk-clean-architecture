using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication
{
    public static class DependencyInjection
    {
        public static AuthenticationSetupBuilder AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration, string defaultScheme)
        {
            var scheme = defaultScheme ?? JwtBearerDefaults.AuthenticationScheme;
            var builder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
            });
            return new AuthenticationSetupBuilder(services, configuration, builder);
        }
    }
}
