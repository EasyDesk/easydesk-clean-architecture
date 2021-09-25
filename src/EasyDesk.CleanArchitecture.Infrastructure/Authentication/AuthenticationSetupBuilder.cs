using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication
{
    public class AuthenticationSetupBuilder
    {
        private readonly AuthenticationBuilder _authenticationBuilder;

        public AuthenticationSetupBuilder(
            IServiceCollection services,
            IConfiguration configuration,
            AuthenticationBuilder authenticationBuilder)
        {
            Services = services;
            Configuration = configuration;
            _authenticationBuilder = authenticationBuilder;
        }

        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }

        public AuthenticationSetupBuilder AddScheme<TOptions, THandler>(string schemeName, Action<TOptions> options)
            where TOptions : AuthenticationSchemeOptions, new()
            where THandler : AuthenticationHandler<TOptions>
        {
            _authenticationBuilder.AddScheme<TOptions, THandler>(schemeName, options ?? (x => { }));
            return this;
        }
    }
}
