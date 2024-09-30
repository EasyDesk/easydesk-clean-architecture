using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
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
    private readonly ApiKeyValidator _apiKeyValidator;

    public ApiKeyHandler(
        IOptionsMonitor<ApiKeyOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApiKeyValidator apiKeyValidator) : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<Result<ClaimsPrincipal>> GetClaimsPrincipalFromToken(string token)
    {
        return await _apiKeyValidator
            .Authenticate(token)
            .ThenMap(agent => new ClaimsPrincipal(agent.ToClaimsIdentity()));
    }
}
