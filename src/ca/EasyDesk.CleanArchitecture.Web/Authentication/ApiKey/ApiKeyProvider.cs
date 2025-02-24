using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

public class ApiKeyProvider : IAuthenticationProvider
{
    private readonly ApiKeyOptions _options;

    public ApiKeyProvider(ApiKeyOptions options)
    {
        _options = options;
    }

    public void AddUtilityServices(ServiceRegistry registry, AppDescription app, string scheme)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddApiKeysManagement(registry, app);

        registry
            .ConfigureContainer(builder =>
            {
                builder.RegisterType<ApiKeyValidator>()
                    .InstancePerLifetimeScope();
            })
            .ConfigureServices(services =>
            {
                services.Configure<SwaggerGenOptions>(options =>
                {
                    var apiKeySecurityScheme = new OpenApiSecurityScheme
                    {
                        Description = $"ApiKey Token Authenticationn ({scheme})",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = ApiKeyOptions.ApiKeyDefaultScheme,
                    };
                    options.ConfigureSecurityRequirement(scheme, apiKeySecurityScheme);
                });
            });
    }

    public IAuthenticationHandler CreateHandler(IComponentContext context, string scheme) => new ApiKeyHandler(
        _options,
        context.Resolve<ApiKeyValidator>(),
        context.Resolve<IHttpContextAccessor>());
}

public class ApiKeyOptions
{
    public const string ApiKeyDefaultScheme = "ApiKey";
    public const string ApiKeyDefaultQueryParameter = "apiKey";

    public TokenReader TokenReader { get; } = TokenReaders.Combine(
        TokenReaders.FromAuthorizationHeader(ApiKeyDefaultScheme),
        TokenReaders.FromQueryParameter(ApiKeyDefaultQueryParameter));
}

internal class ApiKeyHandler : TokenAuthenticationHandler
{
    private readonly ApiKeyOptions _options;
    private readonly ApiKeyValidator _apiKeyValidator;

    public ApiKeyHandler(ApiKeyOptions options, ApiKeyValidator apiKeyValidator, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _options = options;
        _apiKeyValidator = apiKeyValidator;
    }

    protected override Option<string> ReadToken(HttpContext httpContext) =>
        _options.TokenReader(httpContext);

    protected override async Task<Result<Agent>> ValidateToken(string token) =>
        await _apiKeyValidator.Authenticate(token);

    protected override string GetErrorMessage(Error error) => error switch
    {
        ApiKeyTooLong => "The given API key is too long.",
        InvalidApiKey => "The given API key is invalid.",
        _ => "Unknown error",
    };
}
