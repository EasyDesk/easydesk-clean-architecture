using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public interface IAuthenticationHandler
{
    Task<Option<AuthenticationResult>> Authenticate();
}
