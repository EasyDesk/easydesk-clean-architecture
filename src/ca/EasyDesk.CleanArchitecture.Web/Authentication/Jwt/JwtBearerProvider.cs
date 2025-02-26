using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtBearerProvider : IAuthenticationProvider
{
    public JwtBearerProvider(JwtBearerOptions options)
    {
        Options = options;
    }

    public JwtBearerOptions Options { get; }

    public void AddUtilityServices(ServiceRegistry registry, AppDescription app, string scheme)
    {
        registry
            .ConfigureContainer(builder =>
            {
                builder.RegisterType<JwtFacade>()
                    .SingleInstance()
                    .IfNotRegistered(typeof(JwtFacade));

                builder.RegisterType<JwtLogger>()
                    .SingleInstance()
                    .IfNotRegistered(typeof(JwtLogger));
            })
            .ConfigureServices(services =>
            {
                services.Configure<SwaggerGenOptions>(options =>
                {
                    var jwtSecurityScheme = new OpenApiSecurityScheme
                    {
                        Description = $"JWT Token Authentication ({scheme})",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT",
                    };
                    options.ConfigureSecurityRequirement(scheme, jwtSecurityScheme);
                });
            });
    }

    public IAuthenticationHandler CreateHandler(IComponentContext context, string scheme) => new JwtBearerHandler(
        Options,
        context.Resolve<JwtFacade>(),
        context.Resolve<IHttpContextAccessor>());
}

public class JwtBearerOptions
{
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

internal class JwtBearerHandler : TokenAuthenticationHandler
{
    private readonly JwtBearerOptions _options;
    private readonly JwtFacade _jwtFacade;

    public JwtBearerHandler(JwtBearerOptions options, JwtFacade jwtFacade, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _options = options;
        _jwtFacade = jwtFacade;
    }

    protected override Option<string> ReadToken(HttpContext httpContext) =>
        TokenReaders.Bearer()(httpContext);

    protected override Task<Result<Agent>> ValidateToken(string token) => Task.FromResult(_jwtFacade
        .Validate(token, _options.Configuration.ConfigureBuilder)
        .Map(c => c.ToAgent()));

    protected override string GetErrorMessage(Error error) => error switch
    {
        InvalidJwt(var message) => message,
        _ => "Unknown error",
    };
}
