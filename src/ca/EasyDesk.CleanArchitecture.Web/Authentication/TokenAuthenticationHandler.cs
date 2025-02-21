using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Http;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public abstract class TokenAuthenticationHandler : IAuthenticationHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected TokenAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Option<AuthenticationResult>> Authenticate()
    {
        var token = ReadToken(_httpContextAccessor.HttpContext!);
        return await token.MapAsync(ValidateToken).ThenMap(x => x.Match<AuthenticationResult>(
            success: agent => new AuthenticationResult.Authenticated
            {
                Agent = agent,
            },
            failure: error => new AuthenticationResult.Failed
            {
                ErrorMessage = GetErrorMessage(error),
            }));
    }

    protected abstract Option<string> ReadToken(HttpContext httpContext);

    protected abstract Task<Result<Agent>> ValidateToken(string token);

    protected abstract string GetErrorMessage(Error error);
}
