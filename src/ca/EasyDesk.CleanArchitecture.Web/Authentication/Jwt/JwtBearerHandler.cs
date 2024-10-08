﻿using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Commons.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public sealed class JwtBearerOptions : TokenAuthenticationOptions
{
    public JwtBearerOptions()
    {
        TokenReader = TokenReaders.Bearer();
    }

    public JwtValidationConfiguration Configuration { get; private set; } =
        JwtValidationConfiguration.FromKey(KeyUtils.RandomKey());

    public JwtBearerOptions ConfigureValidationParameters(JwtValidationConfiguration configure)
    {
        Configuration = configure;
        return this;
    }

    public JwtBearerOptions LoadParametersFromConfiguration(
        IConfiguration configuration, string sectionName = JwtConfigurationUtils.DefaultConfigurationSectionName)
    {
        return ConfigureValidationParameters(configuration.GetJwtValidationConfiguration(sectionName));
    }
}

public class JwtBearerHandler : TokenAuthenticationHandler<JwtBearerOptions>
{
    private readonly JwtFacade _jwtFacade;

    public JwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        JwtFacade jwtFacade)
        : base(options, logger, encoder)
    {
        _jwtFacade = jwtFacade;
    }

    protected override Task<Result<ClaimsPrincipal>> GetClaimsPrincipalFromToken(string token)
    {
        return Task.FromResult(_jwtFacade
            .Validate(token, Options.Configuration.ConfigureBuilder)
            .Map(x => new ClaimsPrincipal(x)));
    }
}
