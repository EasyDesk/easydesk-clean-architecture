using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public interface IAuthenticationService
{
    Task<Option<(AuthenticationResult Result, string Scheme)>> Authenticate();
}
