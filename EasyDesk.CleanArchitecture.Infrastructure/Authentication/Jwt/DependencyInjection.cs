using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.Jwt
{
    public static class DependencyInjection
    {
        public static AuthenticationSetupBuilder AddJwtBearer(
            this AuthenticationSetupBuilder builder,
            Action<JwtBearerAuthenticationOptions> options = null)
        {
            builder.Services.AddSingleton<JwtService>();
            return builder.AddScheme<JwtBearerAuthenticationOptions, JwtBearerAuthenticationHandler>(
                JwtBearerDefaults.AuthenticationScheme,
                options);
        }
    }
}
