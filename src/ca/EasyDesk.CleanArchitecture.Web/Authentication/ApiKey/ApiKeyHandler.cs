using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.Commons.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

public sealed class ApiKeyOptions : TokenAuthenticationOptions
{
    public const string ApiKeyDefaultScheme = "ApiKey";
    public const string ApiKeyDefaultQueryParameter = "apiKey";

    public ApiKeyOptions()
    {
        TokenReader = TokenReaders.Combine(
            TokenReaders.FromAuthorizationHeader(ApiKeyDefaultScheme),
            TokenReaders.FromQueryParameter(ApiKeyDefaultQueryParameter));
    }
}

internal class ApiKeyHandler : TokenAuthenticationHandler<ApiKeyOptions>
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyHandler(
        IOptionsMonitor<ApiKeyOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator apiKeyValidator) : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<Result<ClaimsPrincipal>> GetClaimsPrincipalFromToken(string token)
    {
        var agent = await _apiKeyValidator.Authenticate(token);
        return agent.Map(a => new ClaimsPrincipal(a.ToClaimsIdentity()));
    }
}
