﻿using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

public interface IAuthenticationProvider
{
    void AddUtilityServices(IServiceCollection services, AppDescription app);

    void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder);

    void ConfigureOpenApi(string schemeName, SwaggerGenOptions options);
}
