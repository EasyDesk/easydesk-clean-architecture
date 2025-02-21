using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

internal class AuthenticationService : IAuthenticationService
{
    private readonly IEnumerable<AuthenticationScheme> _schemes;

    public AuthenticationService(IEnumerable<AuthenticationScheme> schemes)
    {
        _schemes = schemes;
    }

    public async Task<Option<(AuthenticationResult Result, string Scheme)>> Authenticate()
    {
        foreach (var scheme in _schemes)
        {
            var handlerResult = await scheme.Handler.Authenticate();

            if (handlerResult.IsPresent(out var result))
            {
                return Some((result, scheme.Scheme));
            }
        }

        return None;
    }
}
